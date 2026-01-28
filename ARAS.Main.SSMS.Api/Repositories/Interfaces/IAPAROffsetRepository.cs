using ARAS.Main.SSMS.Api.Models.Dtos;

namespace ARAS.Main.SSMS.Api.Repositories.Interfaces
{
    public interface IAPAROffsetRepository : ICreateReceiptStatusRepository<APAROffsetCreateDto, long>
	{
		Task<APAROffsetRowDto> GetAPAdjustmentsByRequestId(long requestId);
	}
}
