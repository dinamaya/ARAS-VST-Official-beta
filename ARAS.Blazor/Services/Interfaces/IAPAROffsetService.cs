using ARAS.Blazor.Models.DTOs;
using ARAS.Blazor.Repositories.Interfaces;

namespace ARAS.Blazor.Services.Interfaces
{
	public interface IAPAROffsetService 
	{
		Task Create(IEnumerable<APAROffsetAPRowDto> apRows, IEnumerable<APAROffsetARRowDto> arRows, IEnumerable<NoteRowDto> notes);
		Task Update(long requestId, IEnumerable<APAROffsetAPRowDto> apRows, IEnumerable<APAROffsetARRowDto> arRows, IEnumerable<NoteRowDto> notes);
	}
}
