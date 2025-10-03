using ARAS.Blazor.Services.Implementations;
using ARAS.Blazor.Services.Interfaces;
using Microsoft.Identity.Web.UI;
using Radzen;

namespace ARAS.Blazor.App_Code.Globals.Extensions
{
	public static class RadzenConfigExtension
	{
		public static void AddRadzenConfig(this IServiceCollection services)
		{
			services.AddRazorPages().AddMicrosoftIdentityUI();
			services.AddRazorComponents().AddInteractiveServerComponents();
			services.AddRadzenComponents();
			services.AddServerSideBlazor();
			services.AddScoped<DialogService>();
			services.AddScoped<NotificationService>();
			services.AddScoped<TooltipService>();
			services.AddScoped<ContextMenuService>();
			services.AddScoped<ThemeService>();
		}
	}
}
