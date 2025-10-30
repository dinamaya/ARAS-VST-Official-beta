using ARAS.Main.SSMS.Api.Models.Dtos;
using System.Runtime.InteropServices;

namespace ARAS.Main.SSMS.Api.Services.Interfaces
{
	public interface IFileManager
	{
		Task<FileUploadRequirementsDto> GetFileUploadRequirements();
		string GetAttachmentGroupDirectoryByDate(DateTime date);
		string GetAttachmentGroupDirectoryToday();
		Task UploadAttachmentAsync(IFormFile file, string fileName, DateTime date);
		string GetUniqueFileName(IFormFile file, string format, DateTime date);
		string GetUniqueFileName(string fileName, string formattedName, DateTime date);
		Task<AttachmentDownloadDto> DownloadFile(string fileName);
	}
}
