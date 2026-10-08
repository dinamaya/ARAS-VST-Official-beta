namespace ARAS.Blazor.Models.DTOs
{
	public class APAROffsetARRowDto
	{
		public string Id { get; set; }
		public ARRowType RowType { get; set; } = ARRowType.Invoice;
		public double InvoiceAmount { get; set; } = 1_000.00d;
		public string InvoiceNumber { get; set; } = string.Empty;
		public string CustomerName { get; set; } = string.Empty;
		public string CustomerNumber { get; set; } = string.Empty;
		public string? AdjustmentReason { get; set; } = null;
		public double Amount { get; set; } = 0.00d;
		public long? CustomerTrxId { get; set; }
		public long? PaymentScheduleId { get; set; }
		public string? CurrencyCode { get; set; }
		public DateTime? GlAccountingDate { get; set; }

		public APAROffsetARRowDto() { }

		public APAROffsetARRowDto(InvoiceDetailsDto details)
		{
			Id = details.Id;
			Amount = details.ReceiptBalance ?? details.InvoiceBalance;
			InvoiceNumber = details.ReceiptNumber ?? details.InvoiceNumber;
			// For AR, the remaining receipt balance is the amount used to determine
			// whether the Fusion adjustment is LINE or INVOICE.
			InvoiceAmount = details.ReceiptBalance ?? details.ReceiptAmount ?? details.InvoiceAmount;
			CustomerName = details.CustomerName;
			CustomerNumber = details.CustomerNumber;
			CustomerTrxId = details.CustomerTrxId;
			PaymentScheduleId = details.PaymentScheduleId;
			CurrencyCode = details.CurrencyCode;
			GlAccountingDate = details.GlAccountingDate;
		}
	}


	public enum ARRowType
	{
		Invoice,
		Adjustment
	}
}

