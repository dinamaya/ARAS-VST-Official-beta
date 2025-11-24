using ARAS.Blazor.App_Code.Globals;
using ARAS.Blazor.App_Code.Globals.Extensions;
using ARAS.Blazor.Models.Complex;

namespace ARAS.Blazor.Models.DTOs
{
	public class BankChargeRowDto : AdjustmentRow
	{
		public BankChargeRowDto() { }

		public BankChargeRowDto(double adjustmentAmount, string remarks, string reasonCode, InvoiceDetailsDto details) : base(details)
		{
			Guards.ThrowInvalidOperationIf(adjustmentAmount > details.InvoiceAmount, 
				$"The provided Adjustment Amount should not be greater than the invoice amount");

			SetValues((float) adjustmentAmount, remarks, reasonCode);
		}

		public override void SetValues(float adjustmentAmount, string remarks, string reasonCode)
		{
			AdjustmentAmount = adjustmentAmount;
			Remarks = string.IsNullOrEmpty(remarks) ? "Charged " + adjustmentAmount.ToPhp() : remarks;

			AdjustmentActivity = "Bank Charge";
			ReasonCode = reasonCode;
		}
	}
}
