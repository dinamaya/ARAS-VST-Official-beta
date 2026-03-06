namespace ARAS.Blazor.Models.DTOs
{
	public class TransactionRequestRowDto
	{
		public long RequestId { get; set; }
		public string RequestNumber { get; set; }
		public string Requestor { get; set; }
		public string DateRequested { get; set; }
        public string CNCApprover { get; set; }
        public string DateCNCApproved { get; set; }
        public string FSGValidator { get; set; }
        public string DateFSGValidated { get; set; }
        public string FSGApprover { get; set; }
        public string DateFSGApproved { get; set; }
        public string Creator { get; set; }
		public string DateCreated { get; set; }
		public string Status { get; set; }
	}
}
