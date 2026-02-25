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
		Task<bool> IsPostable (long requestId);

		Task<string> GetRequestNumberById (long requestId);

		Task<TransactionRequestRowDto> GetTransactionRequestByRequestId(long requestId);
		Task<RequestUpdateEmailDetailsDto> GetForEmailDetailsById (long requestId);
        Task<int> GetPendingRequestCount(string userId);
        Task<int> GetApprovedRequestCount(string userId);
        Task<int> GetDeclinedRequestCount(string userId);
        Task<int> GetResubmittedRequestCount(string userId);
        Task<int> GetPostedRequestCount(string userId);
		Task<int> GetRejectedRequestCount(string userId);

        Task Approve(long requestId, string modifiedBy);
		Task Decline(long requestId, string modifiedBy);
		Task Reject(long requestId, string modifiedBy);
		Task<string> InvoiceExistingAdjustments(string invoiceNumber, IEnumerable<string> adjustmentTypeIds);
	}
}