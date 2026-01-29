using ARAS.Main.SSMS.Api.Models.Dtos;

namespace ARAS.Main.SSMS.Api.Repositories.Interfaces
{
    public interface IAPAROffsetRepository : ICreateReceiptStatusRepository<APAROffsetCreateDto, long>
	{
		Task<long> Update(long requestId, RequestCreationDto<IEnumerable<APAROffsetCreateDto>> data, string modifiedBy);
		Task<APAROffsetRowDto> GetAPAdjustmentsByRequestId(long requestId);
	}
}
