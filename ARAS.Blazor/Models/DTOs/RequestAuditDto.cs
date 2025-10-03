namespace ARAS.Blazor.Models.DTOs
{
	public class RequestAuditDto
	{
		public string Requestor { get; set; }
		public string? Approver { get; set; }
		public string? Validator { get; set; }
		public string? Checker { get; set; }
		public string? ReferenceNumber { get; set; }
		public string? DateRequested { get; set; }
	}
}
