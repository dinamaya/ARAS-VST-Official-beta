using ARAS.Main.SSMS.Api.Context;
using Microsoft.EntityFrameworkCore;

namespace ARAS.Main.SSMS.Api.App_Code.Globals.Extensions
{
	public static class SQLConfigExtension
	{
		public static void AddSQLConfiguration(this IServiceCollection services)
		{

			services.AddDbContext<MainDbContext>((provider, opt) =>
			{
				var connString = provider.GetRequiredService<IConfiguration>().GetConnectionString("MainDbContext") ?? throw new Exception("Connection string MainDbContext Not Found");
				opt.UseSqlServer(connString);
			});

			services.AddDbContext<HangfireDbContext>((provider, opt) =>
			{
				var connString = provider.GetRequiredService<IConfiguration>().GetConnectionString("HangfireDbContext") ?? throw new Exception("Connection string HangfireDbContext Not Found");
				opt.UseSqlServer(connString);
			});
		}
	}
}
