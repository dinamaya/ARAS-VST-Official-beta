namespace ARAS.Main.SSMS.Api.Models.Dtos
{
    public class APAROffsetARRowDto
    {
        public string Id { get; set; }
        public ARRowType RowType { get; set; } = ARRowType.Invoice;
        public double InvoiceAmount { get; set; } = 0.00d;
        public string InvoiceNumber { get; set; } = string.Empty;
        public string? AdjustmentReason { get; set; } = null;
        public double Amount { get; set; } = 0.00d;

		public string CustomerName { get; set; } = string.Empty;
		public string CustomerNumber { get; set; } = string.Empty;
        public long? CustomerTrxId { get; set; }
        public long? PaymentScheduleId { get; set; }
        public string? CurrencyCode { get; set; }
        public DateTime? GlAccountingDate { get; set; }
	}

    public enum ARRowType
    {
        Invoice,
        Adjustment
    }
}
