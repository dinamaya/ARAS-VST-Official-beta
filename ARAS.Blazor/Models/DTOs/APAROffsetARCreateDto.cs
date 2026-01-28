namespace ARAS.Blazor.Models.DTOs
{
    public class APAROffsetARCreateDto
    {
        public ARRowType RowType { get; set; }
        public string InvoiceNumber { get; set; }
        public string? ReasonCodes{ get; set; }
        public double Amount { get; set; }
    }
}