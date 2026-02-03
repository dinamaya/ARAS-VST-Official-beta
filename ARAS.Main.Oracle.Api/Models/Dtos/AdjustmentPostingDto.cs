namespace ARAS.Main.Oracle.Api.Models.Dtos
{
	public record AdjustmentPostingDto
	{
		public long AdjustmentId { get; set; } // Header ID
		public string InvoiceNumber { get; set; }
		public double AdjustmentAmount { get; set; } // Amount
		
		//public string CreatedFrom { get; set; } Default to ADJUSTMENT API

		public DateTime InvoiceDate { get; set; } // GL Date
		public int? PaymentScheduleId { get; set; } = null;
		public DateTime DateApplied { get; set; } // Date Approved / Created 
		public string AdjustmentActivity { get; set; }	// Map to Receivables TRX ID
		public string ReasonCode { get; set; }
		public string Remarks { get; set; } // Comments
		public string CustomerName { get; set; } // Account Name
		public string CustomerNumber { get; set; } // Account Number
	}
}
