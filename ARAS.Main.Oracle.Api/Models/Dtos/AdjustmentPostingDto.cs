namespace ARAS.Main.Oracle.Api.Models.Dtos
{
	public record AdjustmentPostingDto
	{
		public long AdjustmentId { get; set; }
		public long CustomerTRXId { get; set; }
		public string InvoiceNumber { get; set; }
		public double AdjustmentAmount { get; set; }
		public DateTime GLDate { get; set; }
		public int? PaymentScheduleId { get; set; } = null;
		public DateTime DateApplied { get; set; }
		public string TransactionTypeId { get; set; }
		public string ReasonCode { get; set; }
		public string Remarks { get; set; }
		public string AccountName { get; set; }
		public string AccountNumber { get; set; }
	}
}
