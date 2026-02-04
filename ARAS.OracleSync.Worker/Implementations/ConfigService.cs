using ARAS.OracleSync.Worker.Interfaces;

namespace ARAS.OracleSync.Worker.Implementations
{
	public class ConfigService : IConfigService
	{
		private readonly IConfiguration _config;

		public ConfigService(IConfiguration config)
		{
			_config = config;
		}

		public string GetOracleAdjustmentsApiUrl(string route = null) =>
			$"{_config.GetValue<string>("ApiConfig:external:OracleApi:Adjustments")}{route}";

		public string GetRequestsApiUrl(string route = null) =>
			$"{_config.GetValue<string>("ApiConfig:external:MainApi:Requests")}{route}";

		public string GetApprovalsUrl(string route = null) =>
			$"{_config.GetValue<string>("ApiConfig:external:MainApi:Approvals")}{route}";

        public string GetSSMSAdjustmentsApiUrl(string route = null) =>
			$"{_config.GetValue<string>("ApiConfig:external:MainApi:Adjustments")}{route}";

        public int GetRefreshTime() => _config.GetValue<int>("WorkerConfig:Milleseconds");
    }
}
