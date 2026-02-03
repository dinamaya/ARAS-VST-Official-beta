using ARAS.Main.SSMS.Api.Models.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ARAS.Main.SSMS.Api.Repositories.Interfaces
{
    public interface IBaseReceiptAdjustmentRepository : 
		ICreateReceiptStatusRepository<BaseReceiptAdjustmentCreateDto, IEnumerable<ReceiptAdjustmentCreateResponseDto>>,
		IUpdateRepository<ReceiptAdjustmentUpdateRequestDto, long>
	{
		Task<ReceiptAdjustmentUpdateResponseDto> GetDetailsById(long requestId);
		Task<IEnumerable<AdjustmentPostingDto>> GetStagingData(long requestId);
	}
}
