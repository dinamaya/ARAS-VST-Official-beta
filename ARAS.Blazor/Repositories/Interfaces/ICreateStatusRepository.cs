using ARAS.Blazor.Models.DTOs;

namespace ARAS.Blazor.Repositories.Interfaces
{
	public interface ICreateStatusRepository<TRow>
	{
		Task Create(IEnumerable<TRow> rows, IEnumerable<NoteRowDto> notes);
		Task Update(long requestId, IEnumerable<TRow> rows, IEnumerable<NoteRowDto> notes);

		Task Approve(long requestId, IEnumerable<NoteRowDto> notes);
		Task Validate(long requestId, IEnumerable<NoteRowDto> notes);
		Task Decline(NegateRequestDto createDecline, IEnumerable<NoteRowDto> notes);
		Task Reject(NegateRequestDto createReject, IEnumerable<NoteRowDto> notes);
	}
}
