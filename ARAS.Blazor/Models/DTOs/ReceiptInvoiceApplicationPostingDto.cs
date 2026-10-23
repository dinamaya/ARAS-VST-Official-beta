namespace ARAS.Blazor.Models.DTOs
{
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
