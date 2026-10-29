namespace ARAS.OracleSync.Worker.Models.DTOs
{
	public record AdjustmentPostingDto
	{
        public long HeaderId { get; set; }
        public long AdjustmentId { get; set; }
		public string InvoiceNumber { get; set; }
		public double InvoiceAmount { get; set; }
		public double AdjustmentAmount { get; set; }
		public string? CurrencyCode { get; set; }
		public DateTime? GlAccountingDate { get; set; }
		public DateTime InvoiceDate { get; set; }
		public long? PaymentScheduleId { get; set; } = null;
		public DateTime DateApplied { get; set; }
		public string AdjustmentActivity { get; set; }
		public string ReasonCode { get; set; }
		public string Remarks { get; set; }
		public string CustomerName { get; set; }
		public string CustomerNumber { get; set; }
		public long? CustomerTrxId { get; set; }
		public long? ReceiptId { get; set; }
		public bool ApplyReceiptEnabled { get; set; }
		public IList<ReceiptInvoiceApplicationPostingDto> ReceiptApplications { get; set; } = [];
	}

	public class ReceiptInvoiceApplicationPostingDto
	{
		public long? InvoiceId { get; set; }
		public string InvoiceNumber { get; set; } = string.Empty;
		public long PaymentScheduleId { get; set; }
		public double AppliedAmount { get; set; }
		public double? DiscountAmount { get; set; }
		public double AmountDue { get; set; }
		public string CurrencyCode { get; set; } = string.Empty;
	}
}
