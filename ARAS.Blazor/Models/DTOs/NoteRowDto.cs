using Microsoft.AspNetCore.Components.Forms;

namespace ARAS.Blazor.Models.DTOs
{
	public class NoteRowDto
	{
		public string Id { get; set; }
		public string Remarks { get; set; }
		public byte[]? AttachmentData { get; set; }
		public string AttachmentName { get; set; }
		public long? AttachmentSize { get; set; }
		public string? DateUploaded { get; set; }
		public string? Uploader { get; set; }
		public string? AdjustmentType { get; set; }
	}
}
