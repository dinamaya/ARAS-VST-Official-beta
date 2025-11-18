using ARAS.Blazor.Services.Implementations;
using ARAS.Blazor.Services.Interfaces;

namespace ARAS.Blazor.App_Code.Globals.Extensions
{
	public static class RepositoriesExtension
	{
		public static void AddLocalRepositories(this IServiceCollection services)
		{
			services.AddScoped<IAttachmentService, AttachmentService>();
			services.AddScoped<IAuthService, AuthService>();
			services.AddScoped<IBaseService, BaseService>();
			services.AddScoped<ICashDiscountService, CashDiscountService>();
            services.AddScoped<IAPAROffsetService, APAROffsetService>();
            services.AddScoped<IConfigService, ConfigService>();
			services.AddScoped<IEmailService, EmailService>();
			services.AddScoped<IInvoiceService, InvoiceService>();
			services.AddScoped<INoteService, NoteService>();
			services.AddScoped<IOpsService, OpsService>();
			services.AddScoped<IQueryService, QueryService>();
			services.AddScoped<IRequestService, RequestService>();
			services.AddScoped<ITokenService, TokenService>();
			services.AddScoped<ITransactionService, TransactionService>();
			services.AddScoped<IValidationService, ValidationService>();
		}
	}
}
