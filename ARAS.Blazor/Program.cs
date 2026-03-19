using ARAS.Blazor.App_Code.Globals.Extensions;
using ARAS.Blazor.App_Code.Globals.Middlewares;
using ARAS.Blazor.Components;
using Radzen;

var builder = WebApplication.CreateBuilder(args);

// Force Development environment so exact errors are shown in UAT
builder.Environment.EnvironmentName = "Development";

builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();

// Add services to the container.
builder.Services.AddLocalRepositories();
builder.Services.AddComplexConfiguration();
builder.Services.AddRadzenConfig();
builder.Services.AddAuthConfig();
builder.Services.AddValidationConfig();

builder.Services.AddControllers();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseMiddleware<AccountSecurityHashValidatorMiddleware>();
app.UseAuthorization(); 
app.UseAntiforgery();

app.MapControllers();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AllowAnonymous();

app.AddEndpointConfig();

app.Run();
