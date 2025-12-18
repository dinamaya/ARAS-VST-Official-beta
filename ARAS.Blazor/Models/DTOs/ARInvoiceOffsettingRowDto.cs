using ARAS.Blazor.App_Code.Globals;
using Newtonsoft.Json.Linq;

namespace ARAS.Blazor.Models.DTOs
{
    public class ARInvoiceOffsettingRowDto
    {
        public string Id { get; private set; }
        public string AdjustmentActivity { get; set; } = string.Empty;
        public double InvoiceAmount { get; set; } = 1_000.00d;
        public string InvoiceDate { get; set; } = string.Empty;
        public string InvoiceNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerNumber { get; set; } = string.Empty;
        public string ReasonCode { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;

        public ARInvoiceOffsettingRowDto() { }
        public ARInvoiceOffsettingRowDto(string remarks, InvoiceDetailsDto details)
        {
            Id = Utils.Security.GenerateExtendedGuid("ARIO", 1);
            InvoiceAmount = details.InvoiceAmount;
            InvoiceNumber = details.InvoiceNumber;
            InvoiceDate = details.InvoiceDate.ToString("dd MMM yyyy");
            CustomerName = details.CustomerName;
            CustomerNumber = details.CustomerNumber;
            SetValues( remarks);
        }

        public void SetValues(string remarks)
        {
            Remarks = remarks;
            AdjustmentActivity = "AR Offsetting";
            ReasonCode = "Offsetting";
        }
    }
}
