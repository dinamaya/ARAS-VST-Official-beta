using ARAS.Blazor.App_Code.Globals;
using Newtonsoft.Json.Linq;

namespace ARAS.Blazor.Models.DTOs
{
    public class APAROffsetAPRowDto
    {
        public double InvoiceAmount { get; set; } = 1_000.00d;
        public string InvoiceNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerNumber { get; set; } = string.Empty;

        public APAROffsetAPRowDto(InvoiceDetailsDto details)
        {
            InvoiceAmount = details.InvoiceAmount;
            InvoiceNumber = details.InvoiceNumber;
        }
    }
}
