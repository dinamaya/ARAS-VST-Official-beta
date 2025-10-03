using ARAS.Main.SSMS.Api.Repositories.Implementations;
using ARAS.Main.SSMS.Api.Services.Implementations;
using ARAS.Main.SSMS.Api.Repositories.Interfaces;
using ARAS.Main.SSMS.Api.Services.Interfaces;

namespace ARAS.Main.SSMS.Api.App_Code.Globals.Extensions
{
	public static class RepositoriesConfigExtension
	{
		public static void AddRepositoriesConfig(this IServiceCollection services)
		{
			services.AddScoped<ICashDiscountRepository, CashDiscountRepository>();
			services.AddScoped<IStatusRepository, StatusRepository>();
			services.AddScoped<IRequestRepository, RequestRepository>();

			services.AddScoped<IBackgroundJobService, BackgroundJobService>();
			services.AddScoped<IEmailService, EmailService>();
		}
	}
}
