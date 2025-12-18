using ARAS.Blazor.Models.DTOs;

namespace ARAS.Blazor.Services.Interfaces
{
	public interface IRequestService
	{
		Task<bool> IsApprovable(long requestId);
		Task<bool> IsValidatable(long requestId);
		Task<bool> IsDeclined(long requestId);
		Task<TransactionRequestRowDto> GetRequestDetails(long requestId);
	}
}
