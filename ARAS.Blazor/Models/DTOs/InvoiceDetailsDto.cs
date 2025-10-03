namespace ARAS.Blazor.Models.DTOs
{
	public class InvoiceDetailsDto
	{
		public string Id { get; set; }
		public double InvoiceAmount { get; set; }
		public string InvoiceNumber { get; set; }
		public DateTime InvoiceDate { get; set; }
		public string CustomerName { get; set; }
		public string CustomerNumber { get; set; }
		public string OtherDetails { get; set; }
	}
}
