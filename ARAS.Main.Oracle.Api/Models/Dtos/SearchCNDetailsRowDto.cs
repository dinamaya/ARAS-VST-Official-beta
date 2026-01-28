namespace ARAS.Main.Oracle.Api.Models.Dtos
{
	public record SearchCNDetailsRowDto
	{
		public string Id { get; set; }
		public string CNRef { get; set; } = string.Empty;
		public double CNAmt { get; set; } = 0.00d;
		public double WT { get; set; } = 0.00d;

		public DateTime InvoiceDate { get; set; }
		public string CustomerName { get; set; }
		public string CustomerNumber { get; set; }
	}
}
