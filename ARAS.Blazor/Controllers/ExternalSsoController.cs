using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
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

	public ExternalSsoController(
		IOptions<ExternalSsoOptions> options,
		UserManager<Account> userManager,
		AuthDbContext dbContext,
		ILogger<ExternalSsoController> logger,
		ITokenService tokenService,
		IConfigService config)
	{
		_options = options.Value;
		_userManager = userManager;
		_dbContext = dbContext;
		_logger = logger;
		_tokenService = tokenService;
		_config = config;
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
			return Forbid();
		}

		if (!IsValidAccessKey(accessKey))
		{
			_logger.LogWarning("MIS SSO request contained an invalid access key.");
			return Forbid();
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
			return Forbid();
		}

		// ARAS remains the authorization source of truth.
		var account = await _userManager.FindByEmailAsync(email);

		if (account is null || !account.IsActive)
		{
			_logger.LogWarning(
				"No active ARAS account was found for employee {EmployeeNumber}.",
				employeeNumber);

			return Forbid();
		}

		var assignedRoles = await _userManager.GetRolesAsync(account);

		var arasRoles = assignedRoles
			.Where(role => _options.AllowedRoles.Contains(role))
			.Distinct(StringComparer.OrdinalIgnoreCase)
			.ToList();

		if (arasRoles.Count == 0)
		{
			_logger.LogWarning(
				"ARAS account {Email} has no allowed roles.",
				email);

			return Forbid();
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
