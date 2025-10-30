using ARAS.Blazor.Models.DTOs;

namespace ARAS.Blazor.Services.Interfaces
{
	public interface INoteService
	{
		Task Create(long requestId, IEnumerable<NoteRowDto> notes);
		Task<IList<NoteRowDto>> GetRows(long requestId);
	}
}
