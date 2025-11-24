using ARAS.Blazor.Models.DTOs;

namespace ARAS.Blazor.Services.Interfaces
{
	public interface ITransactionRequestQueries<TRow>
	{
		string BaseUrl { get; }
		string AdjustmentTypeCode { get; }

		Task<IEnumerable<TRow>> GetAdjustments(long requestId);

		Task<IEnumerable<TransactionRequestRowDto>> GetSubmissions();
		Task<IEnumerable<TransactionRequestRowDto>> GetApprovals();
		Task<IEnumerable<TransactionRequestRowDto>> GetValidations();
	}
}
