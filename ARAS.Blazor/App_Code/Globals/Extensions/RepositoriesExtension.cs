using ARAS.Blazor.Repositories.Implementations;
using ARAS.Blazor.Repositories.Interfaces;
using ARAS.Blazor.Services.Implementations;
using ARAS.Blazor.Services.Interfaces;

namespace ARAS.Blazor.App_Code.Globals.Extensions
{
	public static class RepositoriesExtension
	{
		public static void AddLocalRepositories(this IServiceCollection services)
		{
			services.AddScoped<IAccountRepository, AccountRepository>();
			services.AddScoped<IAuthRepository, AuthRepository>();
			services.AddScoped<IEmailRepository, EmailRepository>();

			services.AddScoped<IARInvoiceOffsettingService, ARInvoiceOffsettingService>();
			services.AddScoped<IAPAROffsetService, APAROffsetService>();
			services.AddScoped<IAttachmentService, AttachmentService>();
			services.AddScoped<IAuthService, AuthService>();
			services.AddScoped<IBaseService, BaseService>();
			services.AddScoped<IBankChargeService, BankChargeService>();
			services.AddScoped<ICashDiscountService, CashDiscountService>();
            services.AddScoped<IConfigService, ConfigService>();
			services.AddScoped<IEmailService, EmailService>();
			services.AddScoped<IInvoiceService, InvoiceService>();
			services.AddScoped<INoteService, NoteService>();
			services.AddScoped<IOpsService, OpsService>();
			services.AddScoped<IQueryService, QueryService>();
			services.AddScoped<IRequestService, RequestService>();
			services.AddScoped<ISearchOptionService, SearchOptionService>();
			services.AddScoped<ITokenService, TokenService>();
			services.AddScoped<ITestService, TestService>();
			services.AddScoped<ITransactionService, TransactionService>();
			services.AddScoped<IValidationService, ValidationService>();

			services.AddScoped(typeof(IBaseAdjustmentService<,,>), typeof(BaseAdjustmentService<,,>));
		}
	}
}
