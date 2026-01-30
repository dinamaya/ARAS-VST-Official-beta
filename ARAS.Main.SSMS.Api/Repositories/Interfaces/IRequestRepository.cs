using ARAS.Main.SSMS.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.Models.Entities;

namespace ARAS.Main.SSMS.Api.Repositories.Interfaces
{
	public interface IRequestRepository : 
		IReadSingleRepository<Request, long>, 
		ICreateRepository<string, long>,
		ICreateMultipleRepository<string, long>,
		IGetTransactionRequestsRepository
	{
		Task<bool> IsUpdatable(long requestId);
		Task<bool> IsApprovable(long requestId);
		Task<bool> IsDeclinable (long requestId);
		Task<bool> IsRejectable (long requestId);
		Task<bool> IsDeclined (long requestId);

		Task<string> GetRequestNumberById (long requestId);

		Task<TransactionRequestRowDto> GetTransactionRequestByRequestId(long requestId);
		Task<RequestUpdateEmailDetailsDto> GetForEmailDetailsById (long requestId);

		Task Approve(long requestId, string modifiedBy);
		Task Decline(long requestId, string modifiedBy);
		Task Reject(long requestId, string modifiedBy);
	}
}