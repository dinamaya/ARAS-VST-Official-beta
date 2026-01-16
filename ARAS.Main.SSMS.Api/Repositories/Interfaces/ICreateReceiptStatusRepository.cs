using ARAS.Main.SSMS.Api.Models.Dtos;

namespace ARAS.Main.SSMS.Api.Repositories.Interfaces
{
    public interface ICreateReceiptStatusRepository<TCreate> where TCreate : BaseAdjustmentCreateDto
	{
		Task<long> CreateAsync(RequestCreationDto<TCreate> data, string createdBy);
		Task<long> UpdateAsync(long requestId, RequestCreationDto<TCreate> data, string modifiedBy);
	}
}
