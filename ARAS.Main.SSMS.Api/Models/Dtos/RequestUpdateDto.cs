namespace ARAS.Main.SSMS.Api.Models.Dtos
{
	public record RequestUpdateDto(long RequestId, IEnumerable<string> ToEmail);
}
