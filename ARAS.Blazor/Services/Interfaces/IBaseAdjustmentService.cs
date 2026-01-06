using ARAS.Blazor.Models.DTOs;
using ARAS.Blazor.Repositories.Interfaces;

namespace ARAS.Blazor.Services.Interfaces
{
	public interface IBaseAdjustmentService<TCreate, TRow, TValidation>
	{
		Task<string> GetActivityByCode(string adjustmentTypeCode);

		Task Create(List<TCreate> rows, IEnumerable<NoteRowDto> notes, string route);
		Task Update(long requestId, IEnumerable<TCreate> rows, IEnumerable<NoteRowDto> notes, string route);

		Task Approve(long requestId, IEnumerable<NoteRowDto> notes, string adjustmentTypeCode);
		Task Validate(long requestId, IEnumerable<NoteRowDto> notes, string adjustmentTypeCode);

		Task Decline(NegateRequestDto createDecline, IEnumerable<NoteRowDto> notes, string adjustmentTypeCode);
		Task Reject(NegateRequestDto createReject, IEnumerable<NoteRowDto> notes, string adjustmentTypeCode);

		Task<IEnumerable<TRow>> GetAdjustmentsByRequestIdAndTypeCode(long requestId, string adjustmentTypeCode);
		
		Task<IEnumerable<TransactionRequestRowDto>> GetSubmissions(string adjustmenTypeCode);
		Task<IEnumerable<TransactionRequestRowDto>> GetApprovals(string adjustmenTypeCode);
		Task<IEnumerable<TransactionRequestRowDto>> GetValidations(string adjustmenTypeCode);

		Task<bool> IsValid(string adjustmentTypeCode, TValidation inputValues);
	}
}
