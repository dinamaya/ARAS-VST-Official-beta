using ARAS.Main.SSMS.Api.Services.Interfaces;

namespace ARAS.Main.SSMS.Api.Services.Implementations
{
	public class ConfigurationService(IConfiguration configuration) : IConfigurationService
	{
		public string GetFrontendBaseUrl(string? route = "") => $"{configuration.GetValue<string>("FrontendConfig:BaseUrl")}{route}";
	}
}
