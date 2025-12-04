using ARAS.Blazor.App_Code.Globals;
using ARAS.Blazor.App_Code.Globals.Extensions;
using ARAS.Blazor.Models.Complex;
using Newtonsoft.Json.Linq;

namespace ARAS.Blazor.Models.DTOs
{
	public class ARInvoiceOffsettingRowDto : AdjustmentRow
	{
		public ARInvoiceOffsettingRowDto() { }

		public ARInvoiceOffsettingRowDto(string remarks, string reasonCode, InvoiceDetailsDto details) :base(details)
		{
			SetValues(0, remarks, reasonCode);
		}

		public override void SetValues(float discountValue, string remarks, string reasonCode)
		{
			AdjustmentAmount = InvoiceAmount;
			Remarks = remarks;
			AdjustmentActivity = "AR Invoice Offsetting";
			ReasonCode = reasonCode;
		}
	}
}
