namespace ARAS.Blazor.Models.DTOs
{
	public class SRAutoNetRowDto
	{
		public string Id { get; set; }
		public string GLDate { get; set; }
		public double AdjustmentAmount { get; set; }
		public string AdjustmentActivity { get; set; } = string.Empty;
		public string InvoiceNumber { get; set; } = string.Empty;
		public string CustomerName { get; set; } = string.Empty;
		public string CustomerNumber { get; set; } = string.Empty;
		public string ReasonCode { get; set; } = string.Empty;
		public SRAutoNetRemarksDto Remarks { get; set; }
	}
}
