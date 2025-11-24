using ARAS.Blazor.App_Code.Globals.Constants;
using ARAS.Blazor.App_Code.Globals.Extensions;
using ARAS.Blazor.Models.Entities;
using ARAS.Blazor.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace ARAS.Blazor.App_Code.Globals.Middlewares
{
	public class AccountSecurityHashValidatorMiddleware(RequestDelegate request)
	{
		public async Task InvokeAsync(HttpContext context)
		{
			var userManager = context.RequestServices.GetRequiredService<UserManager<Account>>();
			var config = context.RequestServices.GetRequiredService<IConfigService>();
			var identity = context.User.Identity ?? throw new InvalidOperationException("User identity is null");
			var email = context.User.GetClaim("preferred_username");
			var tokenHash = context.User.GetClaim(ClaimTypes.Hash);

			if (identity.IsAuthenticated)
			{
				if (email != null && tokenHash != null)
				{
					var user = await userManager.FindByEmailAsync(email);
					if (user != null && user.SecurityHash != tokenHash)
					{
						context.Response.Cookies.Delete(config.GetTokenName());
						context.Response.Cookies.Delete(".AspNetCore.Cookies");
						await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
						await context.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);

						context.Response.StatusCode = StatusCodes.Status401Unauthorized;
						string url = config.GetBaseUrl($"auth/login?q={Queries.Auth.SESSION_EXPIRE}");
						context.Response.Redirect(url);
						return;
					}
				}
			}
			else
			{
				if (email != null && tokenHash != null)
				{
					context.Response.Cookies.Delete(config.GetTokenName());
					context.Response.Cookies.Delete(".AspNetCore.Cookies");
				}
			}

			await request(context);
		}
	}
}
