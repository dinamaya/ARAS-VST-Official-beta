namespace ARAS.Main.SSMS.Api.Models.Dtos
{
    public class ARInvoiceOffsettingCreateDto
    {
		public long RequestId { get; set; }
		public double InvoiceAmount { get; set; }
		public double AdjustedAmount { get; set; }
		public DateTime InvoiceDate { get; set; }
		public string InvoiceNumber { get; set; }
		public string CustomerName { get; set; }
		public string CustomerNumber { get; set; }
		public string Type { get; set; }
	}
}
