using ARAS.Main.Oracle.Api.Context;
using ARAS.Main.Oracle.Api.Factories.Implementations;
using ARAS.Main.Oracle.Api.Factories.Interfaces;
using ARAS.Main.Oracle.Api.Repositories.Implementations;
using ARAS.Main.Oracle.Api.Repositories.Interfaces;
using ARAS.Main.Oracle.Api.Services.Implementations;
using ARAS.Main.Oracle.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
string mainDbConnection = builder.Configuration.GetConnectionString("MainDbContext") ?? throw new Exception("Main Oracle Database Context not found");

builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
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
builder.Services.AddOpenApi();

builder.Services.AddDbContext<MainDbContext>(options =>
	options.UseOracle(mainDbConnection));

builder.Services.AddScoped<IOracleConnectionFactory, OracleConnectionFactory>();

builder.Services.AddScoped<IConfigurationService, ConfigurationService>();
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<IAdjustmentRepository, AdjustmentRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
