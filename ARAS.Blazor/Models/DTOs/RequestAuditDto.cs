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

        public RequestAuditDto() { }
		public RequestAuditDto(TransactionRequestRowDto transactionRequestRow) 
		{
            Requestor = transactionRequestRow.Requestor;
            Approver = transactionRequestRow.Approver;
            Validator = transactionRequestRow.Validator;
            Checker = transactionRequestRow.Creator;
            ReferenceNumber = transactionRequestRow.RequestNumber;
            DateRequested = transactionRequestRow.DateRequested;
        }

	}
}
