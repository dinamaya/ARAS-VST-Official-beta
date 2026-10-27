namespace ARAS.Main.SSMS.Api.Models.Dtos
{
    public class InvoiceDetailsRequestDto
    {
		public string InvoiceNumber { get; set; }
		public double InvoiceAmount { get; set; }
		public double InvoiceBalance { get; set; }
		public long? CustomerTrxId { get; set; }
		public long? PaymentScheduleId { get; set; }
		public DateOnly InvoiceDate { get; set; }
		public string CustomerName { get; set; }
		public string CustomerNumber { get; set; }
		public string? SourceType { get; set; }
    }
}
