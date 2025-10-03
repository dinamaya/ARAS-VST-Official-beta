using ARAS.Auth.Api.Context;
using ARAS.Auth.Api.Models.Complex;
using ARAS.Auth.Api.Models.Entities;
using ARAS.Auth.Api.Services.Implementations;
using ARAS.Auth.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AuthDbContext>(options =>
{
	options.UseSqlServer(builder.Configuration.GetConnectionString("AuthDbContext")
		?? throw new Exception("Auth Db Context Not Found"));
});
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("AuthConfig:JwtOptions"));
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAccountService, AccountService>();

builder.Services.AddIdentityCore<Account>(options =>
{
	options.User.RequireUniqueEmail = true;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<AuthDbContext>()
.AddSignInManager();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
{
    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

    options.Authority = $"{builder.Configuration["AzureAd:Instance"]}{builder.Configuration["AzureAd:TenantId"]}/v2.0";
    options.ClientId = builder.Configuration["AzureAd:ClientId"];
    options.ClientSecret = builder.Configuration["AzureAd:ClientSecret"];
    options.CallbackPath = builder.Configuration["AzureAd:CallbackPath"];

    options.ResponseType = "code";
    options.SaveTokens = true;

    options.TokenValidationParameters.NameClaimType = "name";
    options.TokenValidationParameters.RoleClaimType = "roles";

    options.Events = new OpenIdConnectEvents
    {
        OnTokenValidated = ctx =>
        {
            var claimsIdentity = (ClaimsIdentity)ctx.Principal.Identity;

            if (!claimsIdentity.HasClaim(c => c.Type == ClaimTypes.Email))
            {
                var email = claimsIdentity.FindFirst("preferred_username")?.Value;
                if (!string.IsNullOrEmpty(email))
                {
                    claimsIdentity.AddClaim(new Claim(ClaimTypes.Email, email));
                }
            }

            return Task.CompletedTask;
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

		ValidIssuer = builder.Configuration["AuthConfig:JwtOptions:Issuer"],
		ValidAudience = builder.Configuration["AuthConfig:JwtOptions:Audience"],
		IssuerSigningKey = new SymmetricSecurityKey(
			Encoding.UTF8.GetBytes(builder.Configuration["AuthConfig:JwtOptions:Key"]))
	};
});

builder.Services.AddControllers();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
