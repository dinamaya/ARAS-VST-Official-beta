namespace ARAS.Blazor.Models.DTOs
{
    public class InvoiceAdjustmentRowDto
    {
		public long RequestId { get; set; }
		public string Requestor { get; set; }
		public string DateRequested { get; set; }
		public string Approver { get; set; }
		public string DateApproved { get; set; }
		// No Validator and DateValidated
		// Remove InvoiceNumbers
		public string Creator { get; set; }
		public string DateCreated { get; set; }
		public string Status { get; set; }
		public string CustomerName { get; set; }
		public string AdjustmentType { get; set; }
		public string ReferencesCount { get; set; } // Either Invoice or Reason Adjustment
		public string InvoiceNumber { get; set; }
	}
}
