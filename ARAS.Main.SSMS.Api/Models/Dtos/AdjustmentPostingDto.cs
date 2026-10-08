namespace ARAS.Main.SSMS.Api.Models.Dtos
{
	public record AdjustmentPostingDto
	{
        public long HeaderId { get; set; } 
        public long AdjustmentId { get; set; }
		public string InvoiceNumber { get; set; }
		public double InvoiceAmount { get; set; } // Original receipt amount
		public double AdjustmentAmount { get; set; }
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
	}
}
