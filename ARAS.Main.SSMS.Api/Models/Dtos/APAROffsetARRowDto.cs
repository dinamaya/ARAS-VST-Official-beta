namespace ARAS.Main.SSMS.Api.Models.Dtos
{
    public class APAROffsetARRowDto
    {
        public string Id { get; set; }
        public ARRowType RowType { get; set; } = ARRowType.Invoice;
        public string InvoiceNumber { get; set; } = string.Empty;
        public string? AdjustmentReason { get; set; } = null;
        public double Amount { get; set; } = 0.00d;

		public string CustomerName { get; set; } = string.Empty;
		public string CustomerNumber { get; set; } = string.Empty;
	}

    public enum ARRowType
    {
        Invoice,
        Adjustment
    }
}
