namespace ARAS.Blazor.Models.DTOs
{
    public class ARInvoiceOffsettingCreateValidationDto
    {
        public string InvoiceNumber { get; set; }
        public double AdjustmentType { get; set; }
        public string Remarks { get; set; }
    }
}
