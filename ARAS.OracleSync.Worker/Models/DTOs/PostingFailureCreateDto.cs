namespace ARAS.OracleSync.Worker.Models.DTOs
{
	public class PostingFailureCreateDto
	{
		public IEnumerable<long> RequestIds { get; set; } = [];
		public string ErrorMessage { get; set; } = string.Empty;
	}
}
