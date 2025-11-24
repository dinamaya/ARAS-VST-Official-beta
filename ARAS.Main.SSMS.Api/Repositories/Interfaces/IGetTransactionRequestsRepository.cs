using ARAS.Main.SSMS.Api.Models.Dtos;

namespace ARAS.Main.SSMS.Api.Repositories.Interfaces
{
	public interface IGetTransactionRequestsRepository
	{
		Task<IEnumerable<TransactionRequestRowDto>> GetAllSubmissionsByType(string adjustmentTypeCode);
		Task<IEnumerable<TransactionRequestRowDto>> GetAllForApprovalsByType(string adjustmentTypeCode);
		Task<IEnumerable<TransactionRequestRowDto>> GetAllForValidationsByType(string adjustmentTypeCode);
	}
}
