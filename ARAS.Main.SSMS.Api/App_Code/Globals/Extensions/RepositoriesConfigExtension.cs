using ARAS.Main.SSMS.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.Repositories.Implementations;
using ARAS.Main.SSMS.Api.Repositories.Interfaces;
using ARAS.Main.SSMS.Api.Services.Implementations;
using ARAS.Main.SSMS.Api.Services.Interfaces;

namespace ARAS.Main.SSMS.Api.App_Code.Globals.Extensions
{
	public static class RepositoriesConfigExtension
	{
		public static void AddRepositoriesConfig(this IServiceCollection services)
		{
			services.AddScoped(typeof(IBaseAdjustmentRepository<>), typeof(BaseAdjustmentRepository<>));

			services.AddScoped<IBaseReceiptAdjustmentRepository, BaseReceiptAdjustmentRepository>();
			services.AddScoped<IAdjustmentRepository, AdjustmentRepository>();
			services.AddScoped<IInvoiceRepository, InvoiceRepository>();
			services.AddScoped<IRequestRepository, RequestRepository>();
			services.AddScoped<IRemarksRepository, RemarksRepository>();
			services.AddScoped<IStatusRepository, StatusRepository>();
			services.AddScoped<ITransactionRepository, TransactionRepository>();

			services.AddScoped<IFileManager, FileManager>();

			services.AddScoped<IAdjustmentService, AdjustmentService>();
			services.AddScoped<INoteService, NoteService>();
			services.AddScoped<IBackgroundJobService, BackgroundJobService>();
			services.AddScoped<IConfigurationService, ConfigurationService>();
		}
	}
}
