namespace ARAS.Main.SSMS.Api.Models.Dtos
{
	public class ARInvoiceOffsettingRowDto
	{
		public string Id { get; set; }
		public string AdjustmentActivity { get; set; } = string.Empty;
		public double InvoiceAmount { get; set; } = 1_000.00d;
		public double InvoiceBalance { get; set; }
		public string InvoiceDate { get; set; } = string.Empty;
		public string InvoiceNumber { get; set; } = string.Empty;
		public string CustomerName { get; set; } = string.Empty;
		public string CustomerNumber { get; set; } = string.Empty;
		public string Type { get; set; } = string.Empty;

		public ARInvoiceOffsettingRowDto()
		{
		}
	}
}
