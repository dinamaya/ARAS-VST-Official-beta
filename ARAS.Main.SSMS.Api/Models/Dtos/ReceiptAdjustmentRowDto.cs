namespace ARAS.Main.SSMS.Api.Models.Dtos
{
    public class ReceiptAdjustmentRowDto
    {
        public long RequestId { get; set; }
		public string Requestor { get; set; }
		public string DateRequested { get; set; }
		public string Approver { get; set; }
		public string DateApproved { get; set; }
		public string AdjustmentTypeCode { get; set; }

		// No Validator and DateValidated
		public string Creator { get; set; }
		public string DateCreated { get; set; }
		public string Status { get; set; }
		public string CustomerName { get; set; }
		public string InvoiceNumber { get; set; }
		public string AdjustmentType { get; set; }
	}
}
