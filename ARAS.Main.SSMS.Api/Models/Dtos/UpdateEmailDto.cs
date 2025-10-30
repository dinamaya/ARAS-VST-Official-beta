namespace ARAS.Main.SSMS.Api.Models.Dtos
{
	public class UpdateEmailDto
	{
		public string RequestId { get; set; }
		public string RequestorName { get; set; }
		public string AdjustmentType { get; set; }
		public string RequestNumber { get; set; }
		public string Status { get; set; }
		public string ForAction { get; set; }
		public string Role { get; set; }
		public string RoleGroup { get; set; }
		public string Endpoint { get; set; }

		public IEnumerable<EmailTimelineDetailsDto> Timeline { get; set; }
		public IEnumerable<string> ToEmail { get; set; }
	}
}
