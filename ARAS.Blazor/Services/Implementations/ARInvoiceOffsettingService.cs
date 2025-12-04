using ARAS.Blazor.Services.Interfaces;
using ARAS.Blazor.Models.DTOs;

namespace ARAS.Blazor.Services.Implementations
{
    public class ARInvoiceOffsettingService :
        BaseAdjustmentCommandService<ARInvoiceOffsettingCreateDto, AROffsettingRowDto, ARInvoiceOffsettingCreateValidationDto>,
        IARInvoiceOffsettingService
    {
        public ARInvoiceOffsettingService(IConfigService configService, IBaseAdjustmentService<ARInvoiceOffsettingCreateDto, AROffsettingRowDto, ARInvoiceOffsettingCreateValidationDto> baseAdjustment) :
    base(baseAdjustment, configService.GetARInvoiceOffsettingUrl(), "cdr", Map)
        {
        }

        private static readonly Func<CashDiscountRowDto, CashDiscountCreateDto> Map = (row) => new()
        {
            DiscountValue = row.DiscountValue,
            InvoiceAmount = row.InvoiceAmount,
            InvoiceDate = DateTime.Parse(row.InvoiceDate),
            InvoiceNumber = row.InvoiceNumber,
            CustomerName = row.CustomerName,
            CustomerNumber = row.CustomerNumber,
            ReasonCode = row.ReasonCode,
            Remarks = row.Remarks,
        };
    }
}
