using ARAS.Blazor.App_Code.Globals;
using ARAS.Blazor.App_Code.Globals.Extensions;
using ARAS.Blazor.Models.Complex;
using Newtonsoft.Json.Linq;

namespace ARAS.Blazor.Models.DTOs
{
	public class CashDiscountRowDto : AdjustmentRow
	{
		public float DiscountValue { get; set; }
		
		public CashDiscountRowDto() { }

		public CashDiscountRowDto(float discountValue, string remarks, string reasonCode, InvoiceDetailsDto details) :base(details)
		{
			SetValues(discountValue, remarks, reasonCode);
		}

		public override void SetValues(float discountValue, string remarks, string reasonCode)
		{
			DiscountValue = discountValue;
			AdjustmentAmount = Math.Round(discountValue * InvoiceAmount, 2);
			Remarks = string.IsNullOrEmpty(remarks) ? (DiscountValue * 100) + "% Discount Remarks" : remarks; ;

			AdjustmentActivity = "Cash Discount";
			ReasonCode = reasonCode;
		}
	}
}
