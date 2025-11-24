using ARAS.Blazor.App_Code.Globals;
using ARAS.Blazor.App_Code.Globals.Extensions;
using ARAS.Blazor.Models.Complex;

namespace ARAS.Blazor.Models.DTOs
{
	public class WriteOffRowDto : AdjustmentRow
	{
		public WriteOffRowDto() { }
		public WriteOffRowDto(double adjustmentAmount, string remarks, string reasonCode, InvoiceDetailsDto details) : base(details)
		{
			Guards.ThrowInvalidOperationIf(adjustmentAmount > details.InvoiceAmount,
				$"The provided Adjustment Amount should not be greater than the invoice amount");

			SetValues((float)adjustmentAmount, remarks, reasonCode);
		}

		public override void SetValues(float adjustmentAmount, string remarks, string reasonCode)
		{
			AdjustmentAmount = adjustmentAmount;
			Remarks = string.IsNullOrEmpty(remarks) ? "Added " + adjustmentAmount.ToPhp() : remarks;

			AdjustmentActivity = "Write Off";
			ReasonCode = reasonCode;
		}
	}
}
