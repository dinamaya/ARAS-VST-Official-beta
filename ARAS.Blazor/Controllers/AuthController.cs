using ARAS.Blazor.App_Code.Globals;
using ARAS.Blazor.App_Code.Globals.Constants;
using ARAS.Blazor.Models.DTOs;
using ARAS.Blazor.Repositories.Interfaces;
using ARAS.Blazor.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ARAS.Auth.Api.Controllers
{
	[Route("api/auth")]
	[ApiController]
	public class AuthController : ControllerBase
	{
		private readonly IAuthRepository _authRepo;
		private readonly IConfigService _config;
		private readonly ILogger<AuthController> _logger;

		public AuthController(IAuthRepository authRepo, IConfigService conifg, ILogger<AuthController> logger, IConfigService configService)
		{
			_authRepo = authRepo;
			_config = conifg;
			_logger = logger;
		}

		[HttpGet("aad/login")]
		public IActionResult Login(string url)
		{
			try
			{
				_logger.LogDebug("Redirection URL=\"" + url + "\"");
				return Challenge(
					new AuthenticationProperties { RedirectUri = $"/api/auth/login-callback?url={Utils.Security.CleanString(url)}" },
					OpenIdConnectDefaults.AuthenticationScheme);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error during AAD login challenge with q=\"" + url + "\"");
				return Redirect(Utils.Security.DecodeString(url) + "?q=ERROR");
			}
		}

		[HttpGet("login-callback")]
		public async Task<IActionResult> LoginCallback(string url)
		{
			string token = "";
			try
			{
				var email = User.FindFirst(ClaimTypes.Email)?.Value ?? User.FindFirst("preferred_username")?.Value;
				var oid = User.FindFirst("oid")?.Value ?? User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;
				var name = User.FindFirst("name")?.Value.Split(" ");

				var signRequest = new AccountSignInRequestDto()
				{
					OpenId = oid,
					Email = email,
					FirstName = name[0],
					LastName = name[1]
				};

				token = await _authRepo.SignInAsync(signRequest);
				string tokenName = _config.GetTokenName();
				var option = new CookieOptions
				{
					HttpOnly = true,
					Secure = true,
					SameSite = SameSiteMode.Strict,
					Path = "/",
					Domain = _config.GetTokenDomainName(),
					Expires = DateTimeOffset.UtcNow.AddHours(8)
				};

				Response.Cookies.Append(tokenName, token, option);
			}
			catch (InvalidOperationException ex)
			{
				_logger.LogError(ex, "Error during AAD login callback with q=\"" + url + "\"");
				return Redirect(Utils.Security.DecodeString(url) + "?q=" + Queries.Auth.INCORRECT);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error during AAD login callback with q=\"" + url + "\"");
				await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
				await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
				return Redirect(Utils.Security.DecodeString(url) + "?q=" + Queries.Auth.UNAUTHORIZED);
			}

			return Redirect(Utils.Security.DecodeString(url));
		}

		[HttpGet("aad/logout")]
		public async Task<IActionResult> Logout(string url)
		{
			try
			{
				await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

				string tokenName = _config.GetTokenName();
				Response.Cookies.Append(tokenName, "", new CookieOptions
				{
					HttpOnly = true,
					Secure = true,
					SameSite = SameSiteMode.Strict,
					Path = "/",
					Domain = _config.GetTokenDomainName(),
					Expires = DateTimeOffset.UtcNow.AddYears(-1)
				});

				var callbackUrl = $"{Request.Scheme}://{Request.Host}/api/auth/login-callback?url={Uri.EscapeDataString(url)}";
				var properties = new AuthenticationProperties
				{
					RedirectUri = callbackUrl
				};

				return SignOut(properties, OpenIdConnectDefaults.AuthenticationScheme);
			}
			catch (InvalidOperationException ex)
			{
				return BadRequest(ex.Message);
			}
			catch (Exception ex)
			{
				return Redirect(Utils.Security.DecodeString(url) + "?q=" + Queries.Auth.UNAUTHORIZED);
			}
		}

		[HttpGet("logout-callback")]
		public async Task<IActionResult> LogoutCallback(string url)
		{
			var redirectUrl = Utils.Security.DecodeString(url) ?? "";
			return Redirect(redirectUrl);
		}
	}
}
