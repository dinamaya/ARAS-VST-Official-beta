using ARAS.Blazor.App_Code.Globals;

namespace ARAS.Blazor.Models.DTOs
{
    public class ARInvoiceOffsettingCNDetailsDto
    {
        public string Id { get; set; }
        public string AdjustmentActivity { get; set; } = string.Empty;
        public double InvoiceAmount { get; set; } = 1_000.00d;
        public string InvoiceDate { get; set; } = string.Empty;
        public string InvoiceNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerNumber { get; set; } = string.Empty;
        public string Type { get; set; }

        public ARInvoiceOffsettingCNDetailsDto() { }
        public ARInvoiceOffsettingCNDetailsDto(InvoiceDetailsDto details)
        {
            Id = Utils.Security.GenerateExtendedGuid("ARIO", 1);
            InvoiceAmount = details.InvoiceAmount;
            InvoiceNumber = details.InvoiceNumber;
            InvoiceDate = details.InvoiceDate.ToString("dd MMM yyyy");
            CustomerName = details.CustomerName;
            CustomerNumber = details.CustomerNumber;
        }
    }
}
