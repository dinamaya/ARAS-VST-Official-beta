namespace ARAS.Blazor.Models.DTOs
{
    public class APAROffsetARRowDto
    {
        public string Id { get; set; }
        public ARRowType RowType { get; set; } = ARRowType.Invoice;
        public string InvoiceNumber { get; set; } = string.Empty;
        public string? ReasonCode { get; set; } = null;
        public double Amount { get; set; } = 0.00d;
    }

    public enum ARRowType
    {
        Invoice,
        Adjustment
    }
}

