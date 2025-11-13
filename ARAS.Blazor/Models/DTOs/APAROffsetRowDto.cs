using ARAS.Blazor.App_Code.Globals;
using Newtonsoft.Json.Linq;

namespace ARAS.Blazor.Models.DTOs
{
    public class APAROffsetRowDto
    {
        public double InvoiceAmount { get; set; } = 1_000.00d;
        public string InvoiceNumber { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;

        public APAROffsetRowDto(string remarks, InvoiceDetailsDto details)
        {
            InvoiceAmount = details.InvoiceAmount;
            InvoiceNumber = details.InvoiceNumber;
            SetValues(remarks);
        }

        public AROffsettingRowDto()
        {
            Remarks = string.Empty;
            InvoiceDetails = new InvoiceDetailsDto();
        }

        public void SetValues(string remarks)
        {
            Remarks = remarks;
            AdjustmentActivity = "AR Offsetting";
            ReasonCode = "Offsetting";
        }
    }
}
