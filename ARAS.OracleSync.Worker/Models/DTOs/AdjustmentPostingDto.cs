namespace ARAS.OracleSync.Worker.Models.DTOs
{
	public record AdjustmentPostingDto
	{
        public long HeaderId { get; set; }
        public long AdjustmentId { get; set; }
		public string InvoiceNumber { get; set; }
		public double AdjustmentAmount { get; set; }
		public DateTime InvoiceDate { get; set; }
		public int? PaymentScheduleId { get; set; } = null;
		public DateTime DateApplied { get; set; }
		public string AdjustmentActivity { get; set; }
		public string ReasonCode { get; set; }
		public string Remarks { get; set; }
		public string CustomerName { get; set; }
		public string CustomerNumber { get; set; }
	}
}
