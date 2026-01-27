namespace ARAS.Blazor.Models.DTOs
{
    public class ReceiptAdjustmentRowDto
    {
		public string CustomerName { get; set; }
		public long RequestId { get; set; } // Reference Number
		public string InvoiceNumber { get; set; }
		public string AdjustmentType { get; set; }
		public string AdjustmentTypeCode { get; set; }

		public string Requestor { get; set; }
		public string DateRequested { get; set; }
		public string Approver { get; set; }
		public string DateApproved { get; set; }
		// No Validator and DateValidated
		public string Creator { get; set; }
		public string DateCreated { get; set; }
		public string Status { get; set; }
	}
}
