using ARAS.Main.SSMS.Api.Models.Complex;

namespace ARAS.Main.SSMS.Api.App_Code.Globals.Extensions
{
	public static class ComplexConfigExtension
	{
		public static void AddComplexConfiguration(this IServiceCollection services)
		{
			var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
			services.Configure<EmailServiceConfig>(configuration.GetSection("EmailServiceConfig"));
		}
	}
}
