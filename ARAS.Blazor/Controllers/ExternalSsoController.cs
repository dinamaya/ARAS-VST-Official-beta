using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ARAS.Blazor.App_Code.Globals;
using ARAS.Blazor.App_Code.Globals.Constants;
using ARAS.Blazor.Context;
using ARAS.Blazor.Models.Configuration;
using ARAS.Blazor.Models.Entities;
using ARAS.Blazor.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ARAS.Blazor.Controllers;

[AllowAnonymous]
[Route("Login")]
public sealed class ExternalSsoController : Controller
{
	private static readonly JsonSerializerOptions JsonOptions = new();
	private readonly AuthDbContext _dbContext;
	private readonly ExternalSsoOptions _options;
	private readonly UserManager<Account> _userManager;
	private readonly ILogger<ExternalSsoController> _logger;
	private readonly ITokenService _tokenService;
	private readonly IConfigService _config;
	private readonly IWebHostEnvironment _env;

	public ExternalSsoController(
		IOptions<ExternalSsoOptions> options,
		UserManager<Account> userManager,
		AuthDbContext dbContext,
		ILogger<ExternalSsoController> logger,
		ITokenService tokenService,
		IConfigService config,
		IWebHostEnvironment env)
	{
		_options = options.Value;
		_userManager = userManager;
		_dbContext = dbContext;
		_logger = logger;
		_tokenService = tokenService;
		_config = config;
		_env = env;
	}

	[HttpGet("Index")]
	public IActionResult Index()
	{
		if (User.Identity?.IsAuthenticated == true)
			return Redirect("/");

		if (string.IsNullOrWhiteSpace(_options.ReturnUrl))
			return StatusCode(StatusCodes.Status503ServiceUnavailable);

		return Redirect(_options.ReturnUrl);
	}

	[HttpPost("Index")]
	[IgnoreAntiforgeryToken]
	public async Task<IActionResult> Index(
		[FromForm(Name = "payload")] string? payload,
		[FromForm(Name = "user_directory")] string? userDirectory,
		[FromForm(Name = "access_key")] string? accessKey)
	{
		if (string.IsNullOrWhiteSpace(payload) ||
			string.IsNullOrWhiteSpace(userDirectory) ||
			string.IsNullOrWhiteSpace(accessKey))
		{
			return Reject("Missing payload, user_directory or access_key in the URMS post.");
		}

		if (!IsValidAccessKey(accessKey))
		{
			return Reject("Invalid access key (ExternalSso:AccessKey does not match URMS).");
		}

		using var payloadDocument = JsonDocument.Parse(payload);
		using var userDirectoryDocument = JsonDocument.Parse(userDirectory);

		var user = userDirectoryDocument.RootElement;

		var email = GetString(user, "email");
		var employeeNumber = GetString(user, "employeeNumber");
		var fullName = GetString(user, "fullName");
		var department = GetString(user, "department");
		var status = GetString(user, "status");
		var isDeleted = GetBoolean(user, "isDeleted");

		if (string.IsNullOrWhiteSpace(email) ||
			isDeleted ||
			!string.Equals(status, "Active", StringComparison.OrdinalIgnoreCase))
		{
			return Reject(
				$"URMS user directory rejected: email='{email}', status='{status}', isDeleted={isDeleted}. Email is required and status must be Active.");
		}

		// URMS is the authorization source of truth. ARAS only maps URMS roles
		// to the role names used by its existing authorization policies.
		// Resolve roles first so no local account is created for users who
		// have no ARAS access in URMS.
		var arasRoles = ResolveArasRoles(payloadDocument.RootElement);

		if (arasRoles.Count == 0)
		{
			return Reject(
				$"User {email} has no allowed ARAS role in the URMS payload. Payload: {payload}");
		}

		var account = await _userManager.FindByEmailAsync(email);

		// First SSO login: URMS has granted access, so provision the local
		// ARAS account (same shape the old AAD sign-in created).
		account ??= await ProvisionAccountAsync(email, employeeNumber, fullName);

		if (account is null)
		{
			return Reject($"Could not create the ARAS account for {email}. See the console log for the Identity errors.");
		}

		if (!account.IsActive)
		{
			return Reject($"ARAS account {email} exists but IsActive = 0.");
		}

		HttpContext.Session.SetString(
			ExternalSsoConstants.PayloadSessionKey,
			payload);

		HttpContext.Session.SetString(
			ExternalSsoConstants.UserDirectorySessionKey,
			userDirectory);

		var claims = new List<Claim>
		{
			new(ClaimTypes.Name, fullName ?? email),
			new(ClaimTypes.Email, email),
			new(
				ExternalSsoConstants.ClaimEmployeeNumber,
				employeeNumber ?? string.Empty),
			new(
				ExternalSsoConstants.ClaimFullName,
				fullName ?? string.Empty),
			new(
				ExternalSsoConstants.ClaimDepartment,
				department ?? string.Empty)
		};

		foreach (var role in arasRoles)
		{
			claims.Add(new Claim(ClaimTypes.Role, role));
		}

		var identity = new ClaimsIdentity(
			claims,
			ExternalSsoConstants.SchemeName);

		await HttpContext.SignInAsync(
			ExternalSsoConstants.SchemeName,
			new ClaimsPrincipal(identity),
			new AuthenticationProperties
			{
				IsPersistent = false,
				AllowRefresh = true,
				ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
			});

		// The downstream API gateway still validates the legacy JWT. Issue it
		// alongside the SSO cookie so API calls made by the Blazor app remain
		// authenticated during the SSO migration.
		var apiToken = _tokenService.GenerateToken(account, arasRoles);
		Response.Cookies.Append(
			_config.GetTokenName(),
			apiToken,
			new CookieOptions
			{
				HttpOnly = true,
				Secure = true,
				SameSite = SameSiteMode.Strict,
				Path = "/",
				Domain = _config.GetTokenDomainName(),
				Expires = DateTimeOffset.UtcNow.AddHours(8)
			});

		_logger.LogInformation(
			"MIS SSO login succeeded for employee {EmployeeNumber}.",
			employeeNumber);

		return Redirect("/");
	}

	// Logs why SSO was refused. In Development the reason is also shown in
	// the browser so it can be diagnosed without digging through the console.
	private IActionResult Reject(string reason)
	{
		_logger.LogWarning("MIS SSO login rejected: {Reason}", reason);

		return _env.IsDevelopment()
			? StatusCode(StatusCodes.Status403Forbidden, "MIS SSO login rejected: " + reason)
			: Forbid();
	}

	private async Task<Account?> ProvisionAccountAsync(
		string email,
		string? employeeNumber,
		string? fullName)
	{
		var (firstName, lastName) = SplitFullName(fullName, email);
		var now = DateTime.Now;

		var account = new Account
		{
			Email = email,
			UserName = email,
			OpenId = string.IsNullOrWhiteSpace(employeeNumber)
				? Guid.NewGuid().ToString()
				: employeeNumber,
			FirstName = Utils.Security.CleanString(firstName),
			LastName = Utils.Security.CleanString(lastName),
			GroupCode = string.Empty,
			CreatedBy = "ARAS-MIS-SSO",
			DateCreated = now,
			ModifiedBy = "ARAS-MIS-SSO",
			DateModified = now,
			IsActive = true
		};

		var result = await _userManager.CreateAsync(account);

		if (result.Succeeded)
		{
			_logger.LogInformation(
				"Provisioned ARAS account for {Email} (employee {EmployeeNumber}) on first MIS SSO login.",
				email,
				employeeNumber);

			return account;
		}

		// A parallel request may have created it first.
		var existing = await _userManager.FindByEmailAsync(email);

		if (existing is null)
		{
			_logger.LogError(
				"Failed to provision ARAS account for {Email}: {Errors}",
				email,
				string.Join(", ", result.Errors.Select(e => e.Description)));
		}

		return existing;
	}

	private static (string FirstName, string LastName) SplitFullName(
		string? fullName,
		string email)
	{
		var parts = (fullName ?? string.Empty)
			.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

		return parts.Length switch
		{
			0 => (email.Split('@')[0], string.Empty),
			1 => (parts[0], string.Empty),
			_ => (string.Join(' ', parts[..^1]), parts[^1])
		};
	}

	private List<string> ResolveArasRoles(JsonElement payloadRoot)
	{
		var roles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		if (!payloadRoot.TryGetProperty("permissions", out var permissions) ||
			permissions.ValueKind != JsonValueKind.Array)
			return roles.ToList();

		foreach (var permission in permissions.EnumerateArray())
		{
			string? externalRole = null;

			if (permission.TryGetProperty("applicationRole", out var applicationRole) &&
				applicationRole.ValueKind == JsonValueKind.Object &&
				applicationRole.TryGetProperty("name", out var nestedName))
			{
				externalRole = nestedName.GetString();
			}

			externalRole ??= GetString(permission, "applicationRoleName");

			if (string.IsNullOrWhiteSpace(externalRole))
				continue;

			if (_options.RoleMappings.TryGetValue(externalRole, out var arasRole) &&
				_options.AllowedRoles.Contains(arasRole))
			{
				roles.Add(arasRole);
			}
			else if (_options.AllowedRoles.Contains(externalRole))
			{
				roles.Add(externalRole);
			}
		}

		return roles.ToList();
	}

	[HttpGet("AccessDenied")]
	public IActionResult AccessDenied()
	{
		return StatusCode(StatusCodes.Status403Forbidden);
	}

	[HttpGet("Logout")]
	public Task<IActionResult> LogoutFromBrowser()
	{
		return LogoutAsync();
	}

	[HttpPost("Logout")]
	[IgnoreAntiforgeryToken]
	public Task<IActionResult> Logout()
	{
		return LogoutAsync();
	}

	private async Task<IActionResult> LogoutAsync()
	{
		HttpContext.Session.Clear();

		await HttpContext.SignOutAsync(
			ExternalSsoConstants.SchemeName);

		Response.Cookies.Delete(
			_config.GetTokenName(),
			new CookieOptions
			{
				Path = "/",
				Domain = _config.GetTokenDomainName(),
				Secure = true,
				SameSite = SameSiteMode.Strict
			});

		return Redirect("/");
	}

	private bool IsValidAccessKey(string suppliedAccessKey)
	{
		if (string.IsNullOrWhiteSpace(_options.AccessKey))
			return false;

		var expected = Encoding.UTF8.GetBytes(_options.AccessKey);
		var supplied = Encoding.UTF8.GetBytes(suppliedAccessKey);

		return expected.Length == supplied.Length &&
			   CryptographicOperations.FixedTimeEquals(expected, supplied);
	}

	private static string? GetString(
		JsonElement element,
		string propertyName)
	{
		return element.TryGetProperty(propertyName, out var property) &&
			   property.ValueKind == JsonValueKind.String
			? property.GetString()
			: null;
	}

	private static bool GetBoolean(
		JsonElement element,
		string propertyName)
	{
		return element.TryGetProperty(propertyName, out var property) &&
			   property.ValueKind == JsonValueKind.True;
	}
}
