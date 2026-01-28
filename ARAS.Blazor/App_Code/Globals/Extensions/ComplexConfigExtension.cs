
using ARAS.Blazor.Models.Complex;

namespace ARAS.Blazor.App_Code.Globals.Extensions
{
	public static class ComplexConfigExtension
	{
		public static void AddComplexConfiguration(this IServiceCollection services)
		{
			var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
			services.Configure<JwtOptions>(configuration.GetSection("AuthConfig:JwtOptions"));
		}
	}
}
