namespace ARAS.Blazor.Models.DTOs
{
    public class ARAdjustmentCreateValidationDto
    {
        public string InvoiceNumber { get; set; }
        public double AdjustmentType { get; set; }
        public string Remarks { get; set; }
    }
}
