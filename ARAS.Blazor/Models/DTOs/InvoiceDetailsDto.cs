namespace ARAS.Blazor.Models.DTOs
{
	public class InvoiceDetailsDto
	{
		public string Id { get; set; }
		public double InvoiceAmount { get; set; }
		public string InvoiceNumber { get; set; }
		public DateTime InvoiceDate { get; set; }
		public string CustomerName { get; set; }
		public string CustomerNumber { get; set; }
		public double InvoiceBalance { get; set; }
		public string DataSource { get; set; }
		public long? CustomerTrxId { get; set; }
		public long? PaymentScheduleId { get; set; }
		public string? CurrencyCode { get; set; }
		public DateTime? GlAccountingDate { get; set; }
		public string ReceiptNumber { get; set; }
		public long? ReceiptId { get; set; }
		public DateTime? ReceiptDate { get; set; }
		public double? ReceiptAmount { get; set; }
		public double? ReceiptBalance { get; set; }
		public double? OriginalInvoiceBalance { get; set; }
		public double? OriginalReceiptBalance { get; set; }
	}
}
