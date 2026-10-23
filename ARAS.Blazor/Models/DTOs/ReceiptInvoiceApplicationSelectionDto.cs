namespace ARAS.Blazor.Models.DTOs
{
	public class ReceiptInvoiceApplicationSelectionDto
	{
		public long? InvoiceId { get; set; }
		public string InvoiceNumber { get; set; } = string.Empty;
		public long PaymentScheduleId { get; set; }
		public DateTime? InvoiceDate { get; set; }
		public DateTime? DueDate { get; set; }
		public double InvoiceAmount { get; set; }
		public double AmountDue { get; set; }
		public string CurrencyCode { get; set; } = string.Empty;
		public double AppliedAmount { get; set; }
		public double? DiscountAmount { get; set; }
	}
}
