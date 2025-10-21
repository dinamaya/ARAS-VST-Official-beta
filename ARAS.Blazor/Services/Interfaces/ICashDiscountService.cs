using ARAS.Blazor.Models.DTOs;

namespace ARAS.Blazor.Services.Interfaces
{
	public interface ICashDiscountService
	{
		Task Create(IEnumerable<CashDiscountRowDto> rows);
		Task Update(long requestId, IEnumerable<CashDiscountRowDto> rows);
		Task<IEnumerable<TransactionRequestRowDto>> GetSubmissions();
		Task<IEnumerable<TransactionRequestRowDto>> GetApprovals();
		Task<IEnumerable<TransactionRequestRowDto>> GetValidations();
		Task<IEnumerable<CashDiscountRowDto>> GetAdjustments(long requestId);
		Task<TransactionRequestRowDto> GetRequestDetails(long requestId);
		Task Approve(long requestId);
		Task Decline(CreateDeclineDto createDecline);
		Task Validate(long requestId);
		Task Reject(long requestId);
		Task<bool> IsValid(CashDiscountCreateValidationDto cashDiscountCreateValidation);
	}
}
