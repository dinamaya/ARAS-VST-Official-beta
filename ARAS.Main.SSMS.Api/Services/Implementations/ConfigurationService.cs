using ARAS.Main.SSMS.Api.Services.Interfaces;
using Microsoft.AspNetCore.Routing;

namespace ARAS.Main.SSMS.Api.Services.Implementations
{
	public class ConfigurationService(IConfiguration configuration) : IConfigurationService
	{
		public string GetAttachmentsDirectory(string? route = "") => $"{configuration.GetValue<string>("FileManagerConfig:Storage")}{route}";

		public string GetFrontendBaseUrl(string? route = "") => $"{configuration.GetValue<string>("FrontendConfig:BaseUrl")}{route}";
	}
}
