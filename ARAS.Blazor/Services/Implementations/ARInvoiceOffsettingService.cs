using ARAS.Blazor.Services.Interfaces;
using ARAS.Blazor.Models.DTOs;

namespace ARAS.Blazor.Services.Implementations
{
    public class ARInvoiceOffsettingService
    {
        public ARInvoiceOffsettingService(IConfigService configService)
        {
        }

        private static readonly Func<ARInvoiceOffsettingRowDto, ARInvoiceOffsettingCreateDto> Map = (row) => new()
        {
            InvoiceAmount = row.InvoiceAmount,
            InvoiceDate = DateTime.Parse(row.InvoiceDate),
            InvoiceNumber = row.InvoiceNumber,
            CustomerName = row.CustomerName,
            CustomerNumber = row.CustomerNumber,
            Remarks = row.Remarks,
        };
    }
}
