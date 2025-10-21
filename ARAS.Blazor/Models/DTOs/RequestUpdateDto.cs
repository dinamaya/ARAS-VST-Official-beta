namespace ARAS.Blazor.Models.DTOs
{
	public record RequestUpdateDto(long RequestId, IEnumerable<string> ToEmail);
}
