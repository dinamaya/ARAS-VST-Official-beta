using ARAS.Blazor.App_Code.Globals;
using Newtonsoft.Json.Linq;

namespace ARAS.Blazor.Models.DTOs
{
    public class ARInvoiceOffsettingRowDto
    {
        public string Id { get; set; }
        public string AdjustmentActivity { get; set; } = string.Empty;
        public double InvoiceAmount { get; set; } = 1_000.00d;
        public double InvoiceBalance { get; set; }
        public double AdjustedAmount { get; set; }
        public string InvoiceDate { get; set; } = string.Empty;
        public string InvoiceNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerNumber { get; set; } = string.Empty;
        public string Type { get; set; }

        public ARInvoiceOffsettingRowDto() { }

        public ARInvoiceOffsettingRowDto(InvoiceDetailsDto details)
        {
            Id = Utils.Security.GenerateExtendedGuid("ARIO", 1);
            InvoiceAmount = details.InvoiceAmount;
            InvoiceBalance = details.InvoiceBalance;
            AdjustedAmount = details.InvoiceBalance;
            InvoiceNumber = details.InvoiceNumber;
            InvoiceDate = details.InvoiceDate.ToString("dd MMM yyyy");
            CustomerName = details.CustomerName;
            CustomerNumber = details.CustomerNumber;
            AdjustmentActivity = "AR Invoice Offsetting";
        }
    }
}
