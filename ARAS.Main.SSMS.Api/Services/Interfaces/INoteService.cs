using ARAS.Main.SSMS.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.Repositories.Interfaces;

namespace ARAS.Main.SSMS.Api.Services.Interfaces
{
	public interface INoteService : ICreateRepository<NoteCreateDto, string>
	{
		Task<IEnumerable<NoteRowDto>> GetByRequestId(long requestId);
		Task<AttachmentDto> GetById(string Id);
	}
}
