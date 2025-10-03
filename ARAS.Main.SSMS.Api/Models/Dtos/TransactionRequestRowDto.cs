namespace ARAS.Main.SSMS.Api.Models.Dtos
{
	public class TransactionRequestRowDto
	{
		public long RequestId { get; set; }
		public string RequestNumber { get; set; }
		public string Requestor { get; set; }
		public string DateRequested { get; set; }
		public string Approver { get; set; }
		public string DateApproved { get; set; }
		public string Validator { get; set; }
		public string DateValidated { get; set; }
		public string Checker { get; set; }
		public string DateChecked { get; set; }
		public string Status { get; set; }
	}
}
