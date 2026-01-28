namespace ARAS.Main.SSMS.Api.Models.Dtos
{
    public class ARIAdjustmentCreateDto
    {
        public long RequestId { get; set; }
        public long InvoiceNumber { get; set; }
        public double AdjustmentAmount { get; set; }
        public string Type { get; set; }
    }
}
