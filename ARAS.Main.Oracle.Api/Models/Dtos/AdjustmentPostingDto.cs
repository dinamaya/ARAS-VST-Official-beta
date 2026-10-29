namespace ARAS.Main.Oracle.Api.Models.Dtos
{
	public record AdjustmentPostingDto
	{
        public long HeaderId { get; set; }
        public long AdjustmentId { get; set; } // Source adjustment id
        public string InvoiceNumber { get; set; }
		public double InvoiceAmount { get; set; } // Original receipt amount
		public double AdjustmentAmount { get; set; } // Amount
		public string? CurrencyCode { get; set; }
		public DateTime? GlAccountingDate { get; set; }
		
		//public string CreatedFrom { get; set; } Default to ADJUSTMENT API

		public DateTime InvoiceDate { get; set; } // GL Date
		public long? PaymentScheduleId { get; set; } = null;
		public DateTime DateApplied { get; set; } // Date Approved / Created 
		public string AdjustmentActivity { get; set; }	// Map to Receivables TRX ID
		public string ReasonCode { get; set; }
		public string Remarks { get; set; } // Comments
		public string CustomerName { get; set; } // Account Name
		public string CustomerNumber { get; set; } // Account Number
		public long? CustomerTrxId { get; set; } // Fusion Customer Transaction ID
		public long? ReceiptId { get; set; }
		public bool ApplyReceiptEnabled { get; set; }
		/// <summary>"Receipt" or "Invoice": the document type the request was raised against (Requests.SourceType).</summary>
		public string? SourceType { get; set; }
		public IList<ReceiptInvoiceApplicationPostingDto> ReceiptApplications { get; set; } = [];
	}

	public class ReceiptInvoiceApplicationPostingDto
	{
		public long? InvoiceId { get; set; }
		public string InvoiceNumber { get; set; } = string.Empty;
		public long PaymentScheduleId { get; set; }
		public double AppliedAmount { get; set; }
		public double? DiscountAmount { get; set; }
		public double AmountDue { get; set; }
		public string CurrencyCode { get; set; } = string.Empty;
	}
}
