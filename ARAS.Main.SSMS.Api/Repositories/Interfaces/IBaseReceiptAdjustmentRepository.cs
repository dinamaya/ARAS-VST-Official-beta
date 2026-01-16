using ARAS.Main.SSMS.Api.Models.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ARAS.Main.SSMS.Api.Repositories.Interfaces
{
    public interface IBaseReceiptAdjustmentRepository<TCreate> where TCreate : BaseAdjustmentCreateDto
	{
		Task<long> Create(RequestCreationDto<TCreate> data, string createdBy, string adjustmentTypeCode);
		Task<long> Update(long requestId, RequestCreationDto<TCreate> data, string modifiedBy, string adjustmentTypeCode);
	}
}
