using ARAS.Main.SSMS.Api.Models.Dtos;

namespace ARAS.Main.SSMS.Api.Repositories.Interfaces
{
    public interface IARInvoiceOffsettingRepository :
        ICreateRepository<RequestCreationDto<AdjustmentRequestCreationDto<ARInvoiceOffsettingCreateDto>>, long>,
        IUpdateRepository<RequestCreationDto<AdjustmentRequestCreationDto<ARInvoiceOffsettingCreateDto>>>
    {
    }
}
