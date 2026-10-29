namespace ARAS.Main.Oracle.Api.Models.Dtos
{
	public record AdjustmentPostingDto
	{
        public long HeaderId { get; set; }
        public long AdjustmentId { get; set; }
		public string InvoiceNumber { get; set; }
		public double AdjustmentAmount { get; set; }
		public double InvoiceAmount { get; set; }
		public string? CurrencyCode { get; set; }
		public DateTime? GlAccountingDate { get; set; }
		public DateTime InvoiceDate { get; set; }
		public long? PaymentScheduleId { get; set; } = null;
		public DateTime DateApplied { get; set; }
		public string AdjustmentActivity { get; set; }
		public string ReasonCode { get; set; }
		public string Remarks { get; set; }
		public string CustomerName { get; set; }
		public string CustomerNumber { get; set; }
       public long? CustomerTrxId { get; set; }

		// Apply Receipt fields. Blazor only relays this DTO from the SSMS API to the
		// Oracle API, so any field missing here is silently dropped from the posting.
		// Keep in sync with ARAS.Main.Oracle.Api.Models.Dtos.AdjustmentPostingDto.
		public long? ReceiptId { get; set; }
		public bool ApplyReceiptEnabled { get; set; }
		/// <summary>"Receipt" or "Invoice": the document type the request was raised against (Requests.SourceType).</summary>
		public string? SourceType { get; set; }
		public IList<ARAS.Blazor.Models.DTOs.ReceiptInvoiceApplicationPostingDto> ReceiptApplications { get; set; } = [];
	}
}
