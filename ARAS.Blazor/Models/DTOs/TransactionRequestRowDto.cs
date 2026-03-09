namespace ARAS.Blazor.Models.DTOs
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
        public string Creator { get; set; }
        public string DateCreated { get; set; }
        public string Status { get; set; }
    }
}
