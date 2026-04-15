using ARAS.Main.SSMS.Api.Models.Dtos;

namespace ARAS.Main.SSMS.Api.Repositories.Interfaces
{
	public interface ITransactionRepository: 
		ICreateRepository<TransactionCreateDto, long>,
		ICreateMultipleRepository<TransactionCreateDto, long>
	{
		Task<IEnumerable<TransactionHistoryDto>> GetHistoryByRequestId(long requestId);
		Task<IEnumerable<EmailTimelineDetailsDto>> GetEmailHistoryByRequestId(long requestId);
        Task<IEnumerable<TransactionHistoryDto>> GetLatestTransactions(string userId, string role, int count);
    }
}
