using ARAS.Blazor.Services.Interfaces;
using Microsoft.AspNetCore.Routing;

namespace ARAS.Blazor.Services.Implementations
{
	public class ConfigService : IConfigService
	{
		private readonly IConfiguration _config;

		public ConfigService(IConfiguration config)
		{
			_config = config;
		}

		public string GetAuthApiUrl(string route = null) => $"{_config.GetValue<string>("ApiConfig:external:AuthApi")}{route}";

		public string GetBaseApiUrl(string route = null) => $"{_config.GetValue<string>("ApiConfig:internal")}{route}";

		public string GetMainOracleApiUrl(string route = null) => $"{_config.GetValue<string>("ApiConfig:external:OracleMainApi")}{route}";

		public string GetMainSSMSApiUrl(string route = null) => $"{_config.GetValue<string>("ApiConfig:external:SSMSMainApi")}{route}";

		public string GetTokenName() => _config["ApiConfig:Cookie:Name"] ?? "";
		public string GetTokenDomainName() => _config["ApiConfig:Cookie:Domain"] ?? "";
	}
}
