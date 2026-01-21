using ARAS.Main.SSMS.Api.Models.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ARAS.Main.SSMS.Api.Repositories.Interfaces
{
    public interface IBaseReceiptAdjustmentRepository
	{
		Task<string> Create(RequestCreationDto<IEnumerable<BaseReceiptAdjustmentCreateDto>> data, string createdBy);
		Task<long> Update(long requestId, RequestCreationDto<IEnumerable<BaseReceiptAdjustmentCreateDto>> data, string modifiedBy);
	}
}
