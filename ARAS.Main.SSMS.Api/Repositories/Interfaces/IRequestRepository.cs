using ARAS.Main.SSMS.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.Models.Entities;

namespace ARAS.Main.SSMS.Api.Repositories.Interfaces
{
	public interface IRequestRepository : IReadSingleRepository<Request, long>, ICreateRepository<RequestCreateDto, long>
	{
		Task<bool> IsApprovable(long requestId);
		Task<bool> IsValidatable (long requestId);
		Task<bool> IsDeclinable (long requestId);
		Task<bool> IsRejectable (long requestId);
	}
}
