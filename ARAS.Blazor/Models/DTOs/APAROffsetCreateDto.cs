namespace ARAS.Blazor.Models.DTOs
{
    public class APAROffsetCreateDto
    {
        public long RequestId { get; set; }
		public string InvoiceNumber { get; set; }
        public double Amount { get; set; }
        public double InvoiceAmount { get; set; }
        public string Type { get; set; }
        public string? ReasonCode { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string CustomerName { get; set; }
        public string CustomerNumber { get; set; }
        public long? CustomerTrxId { get; set; }
        public long? PaymentScheduleId { get; set; }
        public string? CurrencyCode { get; set; }
        public DateTime? GlAccountingDate { get; set; }
    }
}
