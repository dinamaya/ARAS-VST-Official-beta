using ARAS.Blazor.Models.DTOs;
using ARAS.Blazor.Repositories.Interfaces;

namespace ARAS.Blazor.Services.Interfaces
{
    public interface IARInvoiceOffsettingService : IBaseAdjustmentCommandRepository<AROffsettingRowDto, ARInvoiceOffsettingCreateDto, ARInvoiceOffsettingCreateValidationDto>
    {
    }
}
