using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ARAS.Main.SSMS.Api.App_Code.Globals.Extensions
{
	public static class AuthConfigExtension
	{
		public static void AddAuthConfig(this IServiceCollection services, WebApplicationBuilder builder)
		{
			services.AddAuthentication(options =>
			{
				options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
			}).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
			{
				options.Events = new JwtBearerEvents
				{
					OnAuthenticationFailed = context =>
					{
						var logger = context.HttpContext.RequestServices
							.GetRequiredService<ILoggerFactory>()
							.CreateLogger("JwtBearer");
						logger.LogError(context.Exception, "JWT authentication failed for {Path}", context.Request.Path);
						return Task.CompletedTask;
					}
				};

				options.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuer = true,
					ValidateAudience = true,
					ValidateLifetime = true,
					ValidateIssuerSigningKey = true,

					ValidIssuer = builder.Configuration["AuthConfig:JwtOptions:Issuer"],
					ValidAudience = builder.Configuration["AuthConfig:JwtOptions:Audience"],
					IssuerSigningKey = new SymmetricSecurityKey(
						Encoding.UTF8.GetBytes(builder.Configuration["AuthConfig:JwtOptions:Key"]))
				};
			});
		}
	}
}
