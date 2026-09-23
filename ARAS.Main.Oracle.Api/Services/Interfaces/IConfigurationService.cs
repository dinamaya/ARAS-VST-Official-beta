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
		string GetFusionReceiptActivityEndpoint();
		string GetFusionReceiptActivitySoapAction();
		string GetFusionReceiptActivityBusinessUnit();
		long GetFusionReceiptActivityPaymentScheduleId();
		long GetFusionAdjustmentOrgId();
		long GetFusionAdjustmentSetOfBooksId();
		long GetFusionAdjustmentCodeCombinationId(string activityName);
		long GetFusionAdjustmentReceivablesTrxId(string activityName);
	}
}
