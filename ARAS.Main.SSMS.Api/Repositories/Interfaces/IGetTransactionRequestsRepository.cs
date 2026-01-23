using ARAS.Main.SSMS.Api.Models.Dtos;

namespace ARAS.Main.SSMS.Api.Repositories.Interfaces
{
	public interface IGetTransactionRequestsRepository
	{
		Task<IEnumerable<ReceiptAdjustmentRowDto>> GetAllForApprovalsByType(SearchRequestDto data);
		Task<IEnumerable<ReceiptAdjustmentRowDto>> GetSubmissions(SearchRequestDto data, string role, string fullname);
	}
}
