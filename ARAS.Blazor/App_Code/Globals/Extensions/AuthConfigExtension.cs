using ARAS.Blazor.Context;
using ARAS.Blazor.Models.DTOs;
using ARAS.Blazor.Models.Entities;
using ARAS.Blazor.Repositories.Interfaces;
using ARAS.Blazor.Services.Implementations;
using ARAS.Blazor.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;

namespace ARAS.Blazor.App_Code.Globals.Extensions
{
	public static class AuthConfigExtension
	{
		public static void AddAuthConfig(this IServiceCollection services)
		{

			var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
			
			services.AddDbContext<AuthDbContext>(options =>
			{
				options.UseSqlServer(configuration.GetConnectionString("AuthDbContext") ?? throw new Exception("Auth Db Context Not Found"));
			});

			services.AddIdentityCore<Account>(options =>
			{
				options.User.RequireUniqueEmail = true;
			})
			.AddRoles<IdentityRole>()
			.AddEntityFrameworkStores<AuthDbContext>()
			.AddSignInManager();

			services.AddAuthentication(options =>
			{
				options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
				options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
			})
			.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
			.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
			{
				options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

				options.Authority = $"{configuration["AzureAd:Instance"]}{configuration["AzureAd:TenantId"]}/v2.0";
				options.ClientId = configuration["AzureAd:ClientId"];
				options.ClientSecret = configuration["AzureAd:ClientSecret"];
				options.CallbackPath = configuration["AzureAd:CallbackPath"];

				options.ResponseType = "code";
				options.SaveTokens = true;

				options.TokenValidationParameters.NameClaimType = "name";
				options.TokenValidationParameters.RoleClaimType = "roles";

				options.Events = new OpenIdConnectEvents
				{
					OnTokenValidated = async ctx =>
					{
						var claimsIdentity = (ClaimsIdentity)ctx.Principal.Identity;
						
						var email = claimsIdentity.FindFirst(ClaimTypes.Email)?.Value
								?? claimsIdentity.FindFirst("preferred_username")?.Value;

						if (email != null)
							claimsIdentity.AddClaim(new Claim(ClaimTypes.Email, email));

						var authRepo = ctx.HttpContext.RequestServices.GetRequiredService<IAuthRepository>();
						
						var oid = claimsIdentity.FindFirst("oid")?.Value ?? claimsIdentity.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;

						var secHash = await authRepo.GetSecurityHashByOId(oid);
						claimsIdentity.AddClaim(new(ClaimTypes.Hash, secHash));
					}
				};
			})
			.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
			{
				options.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuer = true,
					ValidateAudience = true,
					ValidateLifetime = true,
					ValidateIssuerSigningKey = true,

					ValidIssuer = configuration["AuthConfig:JwtOptions:Issuer"],
					ValidAudience = configuration["AuthConfig:JwtOptions:Audience"],
					IssuerSigningKey = new SymmetricSecurityKey(
						Encoding.UTF8.GetBytes(configuration["AuthConfig:JwtOptions:Key"]))
				};
			});

			services.AddHttpClient("AuthApi", (sp, client) =>
			{
				var tokenService = sp.GetRequiredService<ITokenService>();
				var config = sp.GetRequiredService<IConfigService>();
				client.BaseAddress = new Uri(config.GetAuthUrl());
				var token = tokenService.GetToken();
				if (!string.IsNullOrEmpty(token))
				{
					client.DefaultRequestHeaders.Authorization =
						new AuthenticationHeaderValue("Bearer", token);
				}
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
