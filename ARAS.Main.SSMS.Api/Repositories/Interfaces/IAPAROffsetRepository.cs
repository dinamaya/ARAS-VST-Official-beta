using ARAS.Main.SSMS.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.Repositories.Interfaces;
using System.Threading.Tasks;

namespace ARAS.Main.SSMS.Api.Repositories.Interfaces
{
    public interface IAPAROffsetRepository : IBaseAdjustmentCommandRepository<object, APAROffsetCreateDto, object>
	{
		Task<APAROffsetRowDto> GetAPAdjustmentsByRequestId(long requestId);
	}
}
