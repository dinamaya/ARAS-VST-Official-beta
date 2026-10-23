namespace ARAS.Main.Oracle.Api.Models.Dtos
{
	public class ApplicableReceiptInvoiceDto
	{
		public long? InvoiceId { get; set; }
		public string InvoiceNumber { get; set; } = string.Empty;
		public DateTime InvoiceDate { get; set; }
		public DateTime? DueDate { get; set; }
		public long? CustomerAccountId { get; set; }
		public string CustomerCode { get; set; } = string.Empty;
		public string CustomerName { get; set; } = string.Empty;
		public double InvoiceAmount { get; set; }
		public double InvoiceBalance { get; set; }
		public string InvoiceStatus { get; set; } = "Open";
		public string CurrencyCode { get; set; } = string.Empty;
		public long PaymentScheduleId { get; set; }
		public long? OrgId { get; set; }
	}
}
