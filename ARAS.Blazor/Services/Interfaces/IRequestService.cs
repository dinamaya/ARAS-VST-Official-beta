using ARAS.Blazor.Models.DTOs;
using ARAS.Blazor.Repositories.Interfaces;

namespace ARAS.Blazor.Services.Interfaces
{
	public interface IRequestService : ICreateRequestRepository<BaseReceiptAdjustmentCreateDto>
	{
		Task<bool> IsApprovable(long requestId);
		Task<bool> IsValidatable(long requestId);
		Task<bool> IsDeclined(long requestId);
		Task<TransactionRequestRowDto> GetRequestDetails(long requestId);
		Task<IEnumerable<ReceiptAdjustmentRowDto>> GetReceiptAdjustmentRequests(SearchRequestDto data);
		Task<IEnumerable<InvoiceAdjustmentRowDto>> GetInvoiceAdjustmentRequests(SearchRequestDto data);
		Task<IEnumerable<ReceiptAdjustmentRowDto>> GetReceiptAdjustmentSubmissions(SearchRequestDto data);
		Task<IEnumerable<InvoiceAdjustmentRowDto>> GetInvoiceAdjustmentSubmissions(SearchRequestDto data);
		Task ApproveReceiptAdjustmentRequests(IEnumerable<long> data);
		Task RejectReceiptAdjustmentRequests(IEnumerable<long> data);
		Task DeclineReceiptAdjustmentRequests(IEnumerable<long> data);
	}
}
