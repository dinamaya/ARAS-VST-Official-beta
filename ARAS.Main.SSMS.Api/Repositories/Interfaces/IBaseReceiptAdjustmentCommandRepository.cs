using ARAS.Main.SSMS.Api.Models.Dtos;

namespace ARAS.Main.SSMS.Api.Repositories.Interfaces
{
    public interface IBaseReceiptAdjustmentCommandRepository<TCreate> :
		ICreateRepository<RequestCreationDto<TCreate>, long>,
		IUpdateRepository<RequestCreationDto<TCreate>, long>
    {
    }
}
