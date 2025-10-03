using ARAS.Blazor.Services.Implementations;
using ARAS.Blazor.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;

namespace ARAS.Blazor.App_Code.Globals.Extensions
{
	public static class AuthConfigExtension
	{
		public static void AddAuthConfig(this IServiceCollection services)
		{
			services.AddHttpClient("AuthApi", (sp, client) =>
			{
				var tokenService = sp.GetRequiredService<ITokenService>();
				var config = sp.GetRequiredService<IConfigService>();
				client.BaseAddress = new Uri(config.GetAuthApiUrl());
				var token = tokenService.GetToken();
				if (!string.IsNullOrEmpty(token))
				{
					client.DefaultRequestHeaders.Authorization =
						new AuthenticationHeaderValue("Bearer", token);
				}
			});

			services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
			.AddCookie(options =>
			{
				options.LoginPath = "/auth/login"; 
				options.AccessDeniedPath = "/auth/access-denied";
			});

			services.AddAuthorization(options =>
			{
				options.FallbackPolicy = new AuthorizationPolicyBuilder()
					.RequireAuthenticatedUser()
					.Build();
			});

			services.AddScoped<CookieAuthStateProvider>(); 
			services.AddScoped<AuthenticationStateProvider, CookieAuthStateProvider>();
		}
	}
}
