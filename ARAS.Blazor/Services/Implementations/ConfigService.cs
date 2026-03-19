using ARAS.Blazor.Models.DTOs;
using ARAS.Blazor.Services.Interfaces;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json.Linq;

namespace ARAS.Blazor.Services.Implementations
{
	public class ConfigService : IConfigService
	{
		private readonly IConfiguration _config;

		public ConfigService(IConfiguration config)
		{
			_config = config;
		}
		public string GetBaseUrl(string route = null) => 
			$"{_config.GetValue<string>("ApiConfig:internal")}{route}";
		
		public string GetAuthUrl(string route = null) => 
			$"{_config.GetValue<string>("ApiConfig:external:AuthApi:Auth")}{route}";

		public string GetAuthEmailUrl(string route = null) => 
			$"{_config.GetValue<string>("ApiConfig:external:AuthApi:Email")}{route}";

		public string GetOpsUrl(string route = null) => 
			$"{_config.GetValue<string>("ApiConfig:external:AuthApi:Ops")}{route}";

		public string GetOracleInvoiceApiUrl(string route = null) =>
			$"{_config.GetValue<string>("ApiConfig:external:OracleApi:Invoice")}{route}";
		

		public string GetOracleAdjustmentApiUrl(string route = null) =>
			$"{_config.GetValue<string>("ApiConfig:external:OracleApi:Adjustments")}{route}";
		
		public string GetMainSSMSApiUrl(string route = null) => 
			$"{_config.GetValue<string>("ApiConfig:external:SSMSMainApi")}{route}";

		public string GetTokenName() => _config["AuthConfig:Cookie:Name"] ?? "";
		public string GetTokenDomainName() => _config["AuthConfig:Cookie:Domain"] ?? "";

		// -----------------------------
		// MainApi URLs Implementation
		// -----------------------------

		public string GetAdjustmentsUrl(string route = null) =>
			$"{_config.GetValue<string>("ApiConfig:external:MainApi:Adjustments")}{route}";

		public string GetApprovalsUrl(string route = null) =>
			$"{_config.GetValue<string>("ApiConfig:external:MainApi:Approvals")}{route}";

		public string GetCashDiscountsUrl(string route = null) =>
			$"{_config.GetValue<string>("ApiConfig:external:MainApi:CashDiscounts")}{route}";

        public string GetARInvoiceOffsettingUrl(string route = null) =>
            $"{_config.GetValue<string>("ApiConfig:external:MainApi:ARInvoiceOffsetting")}{route}";

        public string GetAPAROffsetsUrl(string route = null) =>
            $"{_config.GetValue<string>("ApiConfig:external:MainApi:APAROffsets")}{route}";

		public string GetBankChargesUrl(string route = null) =>
			$"{_config.GetValue<string>("ApiConfig:external:MainApi:BankCharges")}{route}";

		public string GetDeclinesUrl(string route = null) =>
			$"{_config.GetValue<string>("ApiConfig:external:MainApi:Declines")}{route}";

		public string GetRejectionsUrl(string route = null) =>
			$"{_config.GetValue<string>("ApiConfig:external:MainApi:Rejections")}{route}";

		public string GetRequestsUrl(string route = null) =>
			$"{_config.GetValue<string>("ApiConfig:external:MainApi:Requests")}{route}";

		public string GetTransactionsUrl(string route = null) =>
			$"{_config.GetValue<string>("ApiConfig:external:MainApi:Transactions")}{route}";

		public string GetValidationsUrl(string route = null) =>
			$"{_config.GetValue<string>("ApiConfig:external:MainApi:Validations")}{route}";

		public string GetSmallAmountUrl(string route = null) =>
			$"{_config.GetValue<string>("ApiConfig:external:MainApi:SmallAmount")}{route}";
		
		public string GetSRAutoNetUrl(string route = null) =>
			$"{_config.GetValue<string>("ApiConfig:external:MainApi:SRAutoNet")}{route}";

		public string GetFilesUrl(string route = null) =>
			$"{_config.GetValue<string>("ApiConfig:external:MainApi:Files")}{route}";

		public IEnumerable<string> GetRequstorSearchInvoiceCategories(string? defaultOption = null)
		{
			var items = _config.GetSection("DropdownOptions:SearchCategories:Requestor").Get<IEnumerable<string>>() ?? Enumerable.Empty<string>();
			return items.Prepend(defaultOption ?? "Select Search Category");
		}

		public IEnumerable<string> GetApprovalSearchInvoiceCategories(string? defaultOption = null)
		{
			var items = _config.GetSection("DropdownOptions:SearchCategories:Approval:Text").Get<IEnumerable<string>>() ?? Enumerable.Empty<string>();
			return items.Prepend(defaultOption ?? "Select Search Category");
		}

		public IEnumerable<double> GetDiscountPercentages()
		{
			var items = _config.GetSection("DropdownOptions:DiscountPercentages").Get<IEnumerable<double>>() ?? Enumerable.Empty<double>();
			return items.Prepend(0);
		}

		public bool IsOnTestRequest() => _config.GetValue<bool>("TestConfig:OnTestRequest");
	}
}
