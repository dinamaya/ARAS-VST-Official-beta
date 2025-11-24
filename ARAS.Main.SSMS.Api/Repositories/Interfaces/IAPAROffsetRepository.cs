using ARAS.Main.SSMS.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.Repositories.Interfaces;
using System.Threading.Tasks;

namespace ARAS.Main.SSMS.Api.Repositories.Interfaces
{
    public interface IAPAROffsetRepository :
        IGetTransactionRequestsRepository,
        ICreateRepository<RequestCreationDto<AdjustmentRequestCreationDto<APAROffsetCreateDto>>, long>
    {
		Task<Tuple<IEnumerable<APAROffsetAPRowDto>, IEnumerable<APAROffsetARRowDto> >> GetAPAdjustmentsByRequestId(long requestId);
    }
}
