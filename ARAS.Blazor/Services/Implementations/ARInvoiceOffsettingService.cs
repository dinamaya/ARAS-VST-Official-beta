using ARAS.Blazor.Services.Interfaces;
using ARAS.Blazor.Models.DTOs;

namespace ARAS.Blazor.Services.Implementations
{
    public class ARInvoiceOffsettingService :
        BaseAdjustmentCommandService<ARInvoiceOffsettingCreateDto, ARInvoiceOffsettingRowDto, ARInvoiceOffsettingCreateValidationDto>,
        IARInvoiceOffsettingService
    {
        public ARInvoiceOffsettingService(IConfigService configService, IBaseAdjustmentService<ARInvoiceOffsettingCreateDto, ARInvoiceOffsettingRowDto, ARInvoiceOffsettingCreateValidationDto> baseAdjustment) :
    base(baseAdjustment, configService.GetARInvoiceOffsettingUrl(), "ofr", Map)
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
