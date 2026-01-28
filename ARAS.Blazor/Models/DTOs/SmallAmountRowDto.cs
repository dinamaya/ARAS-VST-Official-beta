using ARAS.Blazor.App_Code.Globals.Extensions;
using ARAS.Blazor.Models.Complex;

namespace ARAS.Blazor.Models.DTOs
{
	public class SmallAmountRowDto : AdjustmentRow
	{
		public SmallAmountRowDto() { }
		public SmallAmountRowDto(double adjustmentAmount, string remarks, string reasonCode, string adjustmentActivity, InvoiceDetailsDto details) : base(details)
		{
			SetValues((float) adjustmentAmount, remarks, reasonCode, adjustmentActivity);
		}

		public override void SetValues(float adjustmentAmount, string remarks, string reasonCode, string adjustmentActivity)
		{
			AdjustmentAmount = adjustmentAmount;
			Remarks = string.IsNullOrEmpty(remarks) ? "SMALL BALANCE" : remarks;

			AdjustmentActivity = adjustmentActivity;
			ReasonCode = reasonCode;
		}
	}
}
