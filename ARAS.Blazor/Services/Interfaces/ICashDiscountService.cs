using ARAS.Blazor.Models.DTOs;

namespace ARAS.Blazor.Services.Interfaces
{
	public interface ICashDiscountService
	{
		Task Create(IEnumerable<CashDiscountRowDto> rows, IEnumerable<NoteRowDto> notes);
		Task Update(long requestId, IEnumerable<CashDiscountRowDto> rows, IEnumerable<NoteRowDto> notes);
		Task Approve(long requestId, IEnumerable<NoteRowDto> notes);
		Task Decline(NegateRequestDto createDecline, IEnumerable<NoteRowDto> notes);
		Task Validate(long requestId, IEnumerable<NoteRowDto> notes);
		Task Reject(NegateRequestDto createReject, IEnumerable<NoteRowDto> notes);

		Task<IEnumerable<TransactionRequestRowDto>> GetSubmissions();
		Task<IEnumerable<TransactionRequestRowDto>> GetApprovals();
		Task<IEnumerable<TransactionRequestRowDto>> GetValidations();
		Task<IEnumerable<CashDiscountRowDto>> GetAdjustments(long requestId);
		Task<TransactionRequestRowDto> GetRequestDetails(long requestId);
		Task<bool> IsValid(CashDiscountCreateValidationDto cashDiscountCreateValidation);
	}
}
