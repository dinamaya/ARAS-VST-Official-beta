namespace ARAS.Main.SSMS.Api.Models.Dtos
{
    public class ARInvoiceOffsettingRowDto
    {
        public string Id { get; set; }
        public string AdjustmentActivity { get; set; } = string.Empty;
        public double InvoiceAmount { get; set; } = 1_000.00d;
        public string InvoiceDate { get; set; } = string.Empty;
        public string InvoiceNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerNumber { get; set; } = string.Empty;
        public string ReasonCode { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;

        public ARInvoiceOffsettingRowDto()
        {
        }

        public ARInvoiceOffsettingRowDto(string remarks)
        {
            SetValues(remarks);
        }

        public void SetValues(string remarks)
        {
            Remarks = remarks;
            AdjustmentActivity = "AR Invoice Offsetting";
            ReasonCode = "Offsetting";
        }
    }
}
