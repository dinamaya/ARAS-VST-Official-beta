namespace ARAS.Blazor.Models.DTOs
{
    public class APAROffsetARRowDto
    {
        // Type of row: Invoice or Adjustment
        public ARRowType RowType { get; set; } = ARRowType.Invoice;

        // Invoice Fields
        public string InvoiceNumber { get; set; } = string.Empty;

        // Adjustment Fields
        public string? AdjustmentReason { get; set; } = null;

        // Common
        public double Amount { get; set; } = 0.00d;
    }

    public enum ARRowType
    {
        Invoice,
        Adjustment
    }
}
