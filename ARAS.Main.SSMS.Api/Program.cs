using ARAS.Main.SSMS.Api.App_Code.Globals.Extensions;
using ARAS.Main.SSMS.Api.Context.Seeders;
using Hangfire;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddValidationConfig();
builder.Services.AddAuthConfig(builder);
// Every endpoint requires a valid ARAS token unless it opts out with [AllowAnonymous].
builder.Services.AddAuthorization(options =>
{
	options.FallbackPolicy = new AuthorizationPolicyBuilder()
		.RequireAuthenticatedUser()
		.Build();
});
builder.Services.AddComplexConfiguration();
builder.Services.AddRepositoriesConfig();
builder.Services.AddControllers();
builder.Services.AddHangfireConfigExtension();
builder.Services.AddSQLConfiguration();

builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.MapOpenApi().AllowAnonymous();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.UseHangfireDashboard(builder.Configuration.GetSection("HangfireConfig:dashboard").Get<string>(), new DashboardOptions()
{
	DashboardTitle = "ARAS Jobs Monitoring"
});
using (var scope = app.Services.CreateScope())
{
    // Run Only in Dev Mode
    // Disable this on production to avoid errors
    // Populate the Database using the Any Development Project.
    if (app.Environment.IsDevelopment())
    {
        var services = scope.ServiceProvider;
        await AdjustmentTypeSeeder.Run(services);
        await StatusSeeder.Run(services);
    }
}

app.Run();
