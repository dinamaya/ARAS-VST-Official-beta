namespace ARAS.Main.Oracle.Api.Models.Dtos
{
    public class InvoiceDetailsRequestDto
    {
		public string InvoiceNumber { get; set; }
		public double InvoiceAmount { get; set; }
		public DateOnly InvoiceDate { get; set; }
		public string CustomerName { get; set; }
		public string CustomerNumber { get; set; }
	}
}
