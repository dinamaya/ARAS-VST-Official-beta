using ARAS.Blazor.App_Code.Globals.Extensions;
using ARAS.Blazor.Components;
using ARAS.Blazor.Models.Configuration;
using Radzen;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.Cookie.Name = "ARAS.ExternalSSO.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});

builder.Services.Configure<ExternalSsoOptions>(
    builder.Configuration.GetSection(ExternalSsoOptions.SectionName));

builder.Services.AddHttpClient();
builder.Services.AddHttpClient("vstecs-aras-client", client =>
{
    client.Timeout = TimeSpan.FromSeconds(60);
});

builder.Services.AddLocalRepositories();
builder.Services.AddComplexConfiguration();
builder.Services.AddRadzenConfig();
builder.Services.AddAuthConfig();
builder.Services.AddValidationConfig();

builder.Services.AddControllers();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthentication();

// Disable this during the initial MIS SSO test because it expects the old JWT hash.
// app.UseMiddleware<AccountSecurityHashValidatorMiddleware>();

app.UseAuthorization();
app.UseAntiforgery();

app.MapControllers();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AllowAnonymous();

app.Run();
