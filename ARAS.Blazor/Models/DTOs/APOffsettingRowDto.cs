using ARAS.Blazor.App_Code.Globals;
using Newtonsoft.Json.Linq;

namespace ARAS.Blazor.Models.DTOs
{
    public class APOffsettingRowDto
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

        public APOffsettingRowDto(float discountValue, string remarks, InvoiceDetailsDto details)
        {
            Id = Utils.Security.GenerateExtendedGuid("CD", 1);
            InvoiceAmount = details.InvoiceAmount;
            InvoiceNumber = details.InvoiceNumber;
            InvoiceDate = details.InvoiceDate.ToString();
            CustomerName = details.CustomerName;
            CustomerNumber = details.CustomerNumber;
            SetValues(discountValue, remarks);
        }

        public void SetValues(float discountValue, string remarks)
        {
            Remarks = remarks;
            AdjustmentActivity = "AP Offsetting";
            ReasonCode = "Offsetting";
        }
    }
}
