namespace ARAS.Main.SSMS.Api.Models.Dtos
{
	public record AttachmentDownloadDto(byte[] FileBytes, string ContentType, string FilePath);
}
