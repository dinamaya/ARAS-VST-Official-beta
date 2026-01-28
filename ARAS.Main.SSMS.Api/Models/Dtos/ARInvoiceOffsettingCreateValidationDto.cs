namespace ARAS.Main.SSMS.Api.Models.Dtos
{
    public class ARInvoiceOffsettingCreateValidationDto
    {
        public string InvoiceNumber { get; set; }
        public double AdjustmentType { get; set; }
        public string Remarks { get; set; }
    }
}
