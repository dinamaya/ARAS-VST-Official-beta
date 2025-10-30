using Microsoft.AspNetCore.Components.Forms;

namespace ARAS.Main.SSMS.Api.Models.Dtos
{
	public class NoteRowDto
	{
		public string? Id { get; set; }
		public string? Remarks { get; set; }
		public IFormFile? AttachmentData { get; set; }
		public string? AttachmentName { get; set; }
		public long? AttachmentSize { get; set; }
		public string? DateUploaded { get; set; }
		public string? Uploader { get; set; }

	}
}
