namespace ARAS.Main.SSMS.Api.Models.Dtos
{
	public class PostingFailureCreateDto
	{
		public IEnumerable<long> RequestIds { get; set; } = [];
		public string ErrorMessage { get; set; } = string.Empty;
	}
}
