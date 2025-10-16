using ARAS.Auth.Api.App_Code.Globals;
using ARAS.Auth.Api.App_Code.Globals.Constants;
using ARAS.Auth.Api.Models.Dtos;
using ARAS.Auth.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

namespace ARAS.Auth.Api.Controllers
{
	[Route("api/auth")]
	[ApiController]
	public class AuthController : ControllerBase
	{
		private readonly IAuthService _authService;
		private readonly IConfiguration _config;

		public AuthController(IAuthService authService, IConfiguration conifg)
		{
			_authService = authService;
			_config = conifg;
		}

		[HttpGet("aad/login")]
		public IActionResult Login(string url)
		{
			try{
				return Challenge(
					new AuthenticationProperties { RedirectUri = $"/api/auth/login-callback?url={Utils.Security.CleanString(url)}" },
					OpenIdConnectDefaults.AuthenticationScheme);
			}
			catch(Exception ex)
			{
				return Redirect(Utils.Security.DecodeString(url) + "?q=" + Queries.Api.INACCESSIBLE);
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
				
				var signRequest =new  AccountSignInRequestDto(){
					OpenId = oid,
					Email = email,
					FirstName = name[0],
					LastName = name[1]
				};

				token = await _authService.SignInAsync(signRequest);
				string tokenName = _config["AuthConfig:Cookie:Name"] ?? "";

				Response.Cookies.Append(tokenName, token, new CookieOptions
				{
					HttpOnly = true,
					Secure = true,
					SameSite = SameSiteMode.None,
					Path = "/",
					Expires = DateTimeOffset.UtcNow.AddHours(8)
				});
			}
			catch (InvalidOperationException ex)
			{
				return BadRequest(ex.Message);
			}
			catch(Exception ex)
			{
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
				await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);

				string tokenName = _config["AuthConfig:Cookie:Name"] ?? "";
				Response.Cookies.Append(tokenName, "", new CookieOptions
				{
					HttpOnly = true,
					Secure = true,
					SameSite = SameSiteMode.None,
					Path = "/",
					Expires = DateTimeOffset.UtcNow.AddYears(-1)
				});

				var callbackUrl = $"{Request.Scheme}://{Request.Host}/api/auth/logout-callback?url={Uri.EscapeDataString(url)}";
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
