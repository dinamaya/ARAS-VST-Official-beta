namespace ARAS.Blazor.Models.DTOs
{
	public class RequestAuditDto
	{
		public string Requestor { get; set; }
        public string? CNCApprover { get; set; }
        public string? FSGValidator { get; set; }
        public string? FSGApprover { get; set; }
        public string? Checker { get; set; }
		public string? ReferenceNumber { get; set; }
		public string? DateRequested { get; set; }

		public RequestAuditDto() { }
		public RequestAuditDto(TransactionRequestRowDto transactionRequestRow) 
		{
			Requestor = transactionRequestRow.Requestor;
            CNCApprover = transactionRequestRow.CNCApprover;
            FSGValidator = transactionRequestRow.FSGValidator;
            FSGApprover = transactionRequestRow.FSGApprover;
            Checker = transactionRequestRow.Creator;
			ReferenceNumber = transactionRequestRow.RequestNumber;
			DateRequested = transactionRequestRow.DateRequested;
		}

	}
}
