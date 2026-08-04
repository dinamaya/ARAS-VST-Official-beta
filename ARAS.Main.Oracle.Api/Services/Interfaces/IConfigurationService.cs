using ARAS.Main.Oracle.Api.Models.Dtos;

namespace ARAS.Main.Oracle.Api.Services.Interfaces
{
	public interface IConfigurationService
	{
		IEnumerable<string> GetReasonCodes();
		bool IsOntest();
		string GetFusionAdjustmentEndpoint();
		string GetFusionAdjustmentUsername();
		string GetFusionAdjustmentPassword();
		string GetFusionAdjustmentSoapAction();
		long GetFusionAdjustmentOrgId();
		long GetFusionAdjustmentSetOfBooksId();
		long GetFusionAdjustmentCodeCombinationId(string activityName);
		long? GetFusionAdjustmentDefaultPaymentScheduleId();
		long GetFusionAdjustmentReceivablesTrxId(string activityName);
	}
}
