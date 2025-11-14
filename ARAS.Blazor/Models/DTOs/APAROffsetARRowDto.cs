namespace ARAS.Blazor.Models.DTOs
{
    public class APAROffsetARRowDto
    {
        public ARRowType RowType { get; set; } = ARRowType.Invoice;
        public string InvoiceNumber { get; set; } = string.Empty;
        public string? AdjustmentReason { get; set; } = null;
        public double Amount { get; set; } = 0.00d;
    }

    public enum ARRowType
    {
        Invoice,
        Adjustment
    }
}
