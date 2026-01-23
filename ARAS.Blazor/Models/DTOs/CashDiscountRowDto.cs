using ARAS.Blazor.App_Code.Globals;
using ARAS.Blazor.App_Code.Globals.Extensions;
using ARAS.Blazor.Models.Complex;
using Newtonsoft.Json.Linq;

namespace ARAS.Blazor.Models.DTOs
{
	public class CashDiscountRowDto : AdjustmentRow
	{
		public CashDiscountRowDto() { }

		public CashDiscountRowDto(double adjustmentAmount, string remarks, string adjustmentActivity, InvoiceDetailsDto details) :base(details)
		{
			SetValues((float) adjustmentAmount, remarks, adjustmentActivity);
		}

		public void SetValues(float adjustmentAmount, string remarks, string adjustmentActivity)
		{
			AdjustmentAmount = adjustmentAmount;
			Remarks = string.IsNullOrEmpty(remarks) ? (Math.Round((adjustmentAmount / InvoiceAmount) / 100, 2)) + "% Discount Remarks" : remarks; ;

			AdjustmentActivity = adjustmentActivity;
		}
	}
}
