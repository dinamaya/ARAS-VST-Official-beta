using ARAS.Blazor.Models.DTOs;
using ARAS.Blazor.Repositories.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ARAS.Blazor.Services.Interfaces
{
	public interface IRequestService
	{
		Task<bool> IsUpdatable(long requestId);
		Task<bool> IsApprovable(long requestId);
		Task<bool> IsValidatable(long requestId);
		Task<bool> IsDeclined(long requestId);
		Task<TransactionRequestRowDto> GetRequestDetails(long requestId);
		Task<ReceiptAdjustmentUpdateResponseDto> GetReceiptDetails(long requestId);
		Task<IEnumerable<ReceiptAdjustmentRowDto>> GetReceiptAdjustmentRequests(SearchRequestDto data);
		Task<IEnumerable<InvoiceAdjustmentRowDto>> GetInvoiceAdjustmentRequests(SearchRequestDto data);
		Task<IEnumerable<ReceiptAdjustmentRowDto>> GetReceiptAdjustmentSubmissions(SearchRequestDto data);
		Task<IEnumerable<InvoiceAdjustmentRowDto>> GetInvoiceAdjustmentSubmissions(SearchRequestDto data);
		Task ApproveReceiptAdjustmentRequests(IEnumerable<long> data);
		Task RejectReceiptAdjustmentRequests(IEnumerable<long> data);
		Task DeclineReceiptAdjustmentRequests(IEnumerable<long> data);

		Task Update(bool isUpdatable, long requestId, ReceiptAdjustmentUpdateRequestDto row, IEnumerable<NoteRowDto> notes);
		Task Create(IEnumerable<BaseReceiptAdjustmentCreateDto> row, IEnumerable<NoteRowDto> notes);
	}
}
