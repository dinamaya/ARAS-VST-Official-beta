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
		Task<string> GetNextApprovalStatus(long requestId, string role);
		Task<string> GetResubmissionStatus(long requestId);

		Task<TransactionRequestRowDto> GetTransactionRequestByRequestId(long requestId);
		Task<RequestUpdateEmailDetailsDto> GetForEmailDetailsById (long requestId);
        Task<int> GetPendingRequestCount(string userId);
        Task<int> GetApprovedRequestCount(string userId);
        Task<int> GetDeclinedRequestCount(string userId);
        Task<int> GetFsgValidationRequestCount(string userId);
        Task<int> GetFsgApprovalRequestCount(string userId);
		Task<int> GetResubmittedRequestCount(string userId);
		Task<int> GetPostedRequestCount(string userId);
		Task<int> GetRejectedRequestCount(string userId);
		Task<ApproverQueueHealthDto> GetApproverQueueHealth(string userId, string role, int lookbackDays = 30, int overdueAfterDays = 2);
		Task<IEnumerable<ReportsDto>> GetReports(ReportFiltersDto filters, string role, string fullName);

        Task Approve(long requestId, string modifiedBy, string role);
		Task Decline(long requestId, string modifiedBy);
		Task Reject(long requestId, string modifiedBy);
		Task<string> InvoiceExistingAdjustments(string invoiceNumber, IEnumerable<string> adjustmentTypeIds);
	}
}
