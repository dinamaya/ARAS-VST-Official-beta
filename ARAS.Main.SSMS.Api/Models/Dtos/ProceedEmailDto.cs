namespace ARAS.Main.SSMS.Api.Models.Dtos
{
	public class ProceedEmailDto
	{
		public string RequestId { get; set; }
		public string RequestorName { get; set; }
		public string AdjustmentType { get; set; }
		public string RequestNumber { get; set; }
		public string Status { get; set; }
		public IEnumerable<EmailTimelineDetailsDto> Timeline { get; set; }
		public IEnumerable<string> ToEmail { get; set; }
	}
}
