using ARAS.Blazor.Models.DTOs;

namespace ARAS.Blazor.Repositories.Interfaces
{
	public interface IRequestSubmissionReaderRepository
	{
		Task<IEnumerable<TransactionRequestRowDto>> GetSubmissions();
		Task<IEnumerable<TransactionRequestRowDto>> GetApprovals();
		Task<IEnumerable<TransactionRequestRowDto>> GetValidations();
	}
}
