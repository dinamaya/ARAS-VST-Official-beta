using ARAS.Blazor.Services.Implementations;
using ARAS.Blazor.Services.Interfaces;

namespace ARAS.Blazor.App_Code.Globals.Extensions
{
  public static class RepositoriesExtension
  {
    public static void AddLocalRepositories(this IServiceCollection services)
    {
      services.AddScoped<IAuthService, AuthService>();
      services.AddScoped<IBaseService, BaseService>();
      services.AddScoped<ICashDiscountService, CashDiscountService>();
      services.AddScoped<IConfigService, ConfigService>();
      services.AddScoped<IInvoiceService, InvoiceService>();
      services.AddScoped<IOpsService, OpsService>();
      services.AddScoped<IRequestService, RequestService>();
      services.AddScoped<ITokenService, TokenService>();
    }
  }
}
