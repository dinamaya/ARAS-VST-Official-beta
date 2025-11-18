using ARAS.Blazor.App_Code.Globals;

namespace ARAS.Blazor.Models.DTOs
{
	public class CashDiscountRowDto
	{
		public float DiscountValue { get; set; }
		public string Id { get; set; }
		public double AdjustmentAmount { get; set; }
		public string AdjustmentActivity { get; set; } = string.Empty;
		public double InvoiceAmount { get; set; } = 1_000.00d;
		public string InvoiceDate { get; set; } = string.Empty;
		public string InvoiceNumber { get; set; } = string.Empty;
		public string CustomerName { get; set; } = string.Empty;
		public string CustomerNumber { get; set; } = string.Empty;
		public string ReasonCode { get; set; } = string.Empty;
		public string Remarks { get; set; } = string.Empty;

		public CashDiscountRowDto() { }

		public CashDiscountRowDto(float discountValue, string remarks, InvoiceDetailsDto details)
		{
			Id = Utils.Security.GenerateExtendedGuid("CD",1);
			InvoiceAmount = details.InvoiceAmount;
			InvoiceNumber = details.InvoiceNumber;
			InvoiceDate = details.InvoiceDate.ToString();
			CustomerName = details.CustomerName;
			CustomerNumber = details.CustomerNumber;
			SetValues(discountValue, remarks);
		}

		public void SetValues(float discountValue, string remarks)
		{
			DiscountValue = discountValue;
			Remarks = remarks;
			AdjustmentAmount = Math.Round(discountValue * InvoiceAmount, 2);

			AdjustmentActivity = "Cash Discount";
			ReasonCode = "Discount";
		}
	}
}
