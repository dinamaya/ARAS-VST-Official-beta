using Hangfire;

namespace ARAS.Main.SSMS.Api.App_Code.Globals.Extensions
{
	public static class HangfireConfigExtension
	{
		public static void AddHangfireConfigExtension(this IServiceCollection services)
		{
			services.AddHangfire((provider, option) => {
				option.UseSimpleAssemblyNameTypeSerializer().UseRecommendedSerializerSettings();
				var connString = provider.GetRequiredService<IConfiguration>().GetConnectionString("HangfireDBContext");
				option.UseSqlServerStorage(connString);
			});

			services.AddHangfireServer();
		}
	}
}
