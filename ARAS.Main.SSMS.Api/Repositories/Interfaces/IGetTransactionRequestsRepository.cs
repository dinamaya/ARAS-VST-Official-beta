using ARAS.Main.SSMS.Api.Models.Dtos;

namespace ARAS.Main.SSMS.Api.Repositories.Interfaces
{
	public interface IGetTransactionRequestsRepository
	{
		Task<IEnumerable<TransactionRequestRowDto>> GetAllSubmissions();
		Task<IEnumerable<TransactionRequestRowDto>> GetAllForApprovals();
		Task<IEnumerable<TransactionRequestRowDto>> GetAllForValidations();
	}
}
