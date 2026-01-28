namespace ARAS.Main.Oracle.Api.Models.Dtos
{
	public class InvoiceAPDetailsDto
	{
		public string Id { get; set; }
		public double InvoiceAmount { get; set; }
		public DateTime InvoiceDate { get; set; }
		public string InvoiceNumber { get; set; }
		public string CustomerName { get; set; }
		public string CustomerNumber { get; set; }
	}
}
