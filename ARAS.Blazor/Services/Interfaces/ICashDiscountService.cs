using ARAS.Blazor.Models.DTOs;

namespace ARAS.Blazor.Services.Interfaces
{
	public interface ICashDiscountService
	{
		Task Create(IEnumerable<CashDiscountRowDto> rows);
		Task<IEnumerable<TransactionRequestRowDto>> GetSubmissions();
		Task<IEnumerable<TransactionRequestRowDto>> GetApprovals();
		Task<IEnumerable<TransactionRequestRowDto>> GetValidations();
		Task<IEnumerable<CashDiscountRowDto>> GetAdjustments(long requestId);
		Task<TransactionRequestRowDto> GetRequestDetails(long requestId);
		Task<bool> ApproveAdjustment(long requestId);
		Task<bool> ValidateAdjustment(long requestId);
	}
}
