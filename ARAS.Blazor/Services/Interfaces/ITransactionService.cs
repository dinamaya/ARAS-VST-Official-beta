using ARAS.Blazor.Models.DTOs;

namespace ARAS.Blazor.Services.Interfaces
{
	public interface ITransactionService
	{
		Task<IEnumerable<TransactionHistoryDto>> GetHistoryByRequestId(long requestId);
	}
}
