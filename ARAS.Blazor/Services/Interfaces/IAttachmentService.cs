using ARAS.Main.SSMS.Api.Models.Dtos;

namespace ARAS.Blazor.Services.Interfaces
{
	public interface IAttachmentService
	{
		Task<FileUploadRequirementsDto> GetFileUploadRequirements();
		Task Download(string noteId, string attachmentName);
	}
}
