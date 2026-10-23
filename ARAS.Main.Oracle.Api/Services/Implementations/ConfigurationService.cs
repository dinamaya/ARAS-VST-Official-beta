using ARAS.Main.Oracle.Api.Models.Dtos;
using ARAS.Main.Oracle.Api.Services.Interfaces;

namespace ARAS.Main.Oracle.Api.Services.Implementations
{
	public class ConfigurationService(IConfiguration config) : IConfigurationService
	{
		public IEnumerable<string> GetReasonCodes() => config.GetSection("DropdownOptions:ReasonCodes").Get<IEnumerable<string>>();
		public bool IsOntest() => config.GetValue<bool>("TestConfig:OnTest");
		public string GetFusionAdjustmentEndpoint() => GetRequiredValue("OracleFusionAdjustmentService:Endpoint");
		public string GetFusionAdjustmentUsername() => GetRequiredValue("OracleFusionAdjustmentService:Username");
		public string GetFusionAdjustmentPassword() => GetRequiredValue("OracleFusionAdjustmentService:Password");
		public string GetFusionAdjustmentSoapAction() => GetRequiredValue("OracleFusionAdjustmentService:SoapAction");
		public string GetFusionReceiptActivityEndpoint() => GetRequiredValue("OracleFusionReceiptActivityService:Endpoint");
		public string GetFusionReceiptActivitySoapAction() => GetRequiredValue("OracleFusionReceiptActivityService:SoapAction");
		public string GetFusionReceiptApplySoapAction() => GetRequiredValue("OracleFusionReceiptActivityService:ApplySoapAction");
		public string GetFusionReceiptApplyRestEndpointTemplate() => GetRequiredValue("OracleFusionReceiptApplicationService:EndpointTemplate");
		public string GetFusionReceiptActivityBusinessUnit() => GetRequiredValue("OracleFusionReceiptActivityService:BusinessUnit");
		public long GetFusionReceiptActivityPaymentScheduleId() => GetRequiredLong("OracleFusionReceiptActivityService:AppliedPaymentScheduleId");
		public long GetFusionAdjustmentOrgId() => GetRequiredLong("OracleFusionAdjustmentService:OrgId");
		public long GetFusionAdjustmentSetOfBooksId() => GetRequiredLong("OracleFusionAdjustmentService:SetOfBooksId");
		public long GetFusionAdjustmentCodeCombinationId(string activityName)
		{
			var section = config.GetSection("OracleFusionAdjustmentService:ReceivablesActivities");
			var value = section.GetSection(activityName)["CodeCombinationId"];

			if (long.TryParse(value, out var id))
				return id;

			throw new InvalidOperationException($"CodeCombinationId is not configured for adjustment activity '{activityName}'.");
		}

		public long GetFusionAdjustmentReceivablesTrxId(string activityName)
		{
			var section = config.GetSection("OracleFusionAdjustmentService:ReceivablesActivities");
			var value = section.GetSection(activityName)["ReceivablesTrxId"];

			if (long.TryParse(value, out var id))
				return id;

			throw new InvalidOperationException($"ReceivablesTrxId is not configured for adjustment activity '{activityName}'.");
		}

		private string GetRequiredValue(string key)
		{
			var value = config[key];
			if (!string.IsNullOrWhiteSpace(value))
				return value;

			throw new InvalidOperationException($"Missing configuration value: {key}");
		}

		private long GetRequiredLong(string key)
		{
			var value = GetRequiredValue(key);
			if (long.TryParse(value, out var result))
				return result;

			throw new InvalidOperationException($"Invalid numeric configuration value: {key}");
		}

		private long? GetOptionalLong(string key)
		{
			var value = config[key];
			if (string.IsNullOrWhiteSpace(value))
				return null;

			if (long.TryParse(value, out var result))
				return result;

			throw new InvalidOperationException($"Invalid numeric configuration value: {key}");
		}

	}
}
