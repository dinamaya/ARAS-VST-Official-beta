namespace ARAS.Main.SSMS.Api.Models.Dtos
{
	public class CashDiscountCreateDto
	{
		public float DiscountValue { get; set; }
		public double InvoiceAmount { get; set; }
		public DateTime InvoiceDate { get; set; }
		public string InvoiceNumber { get; set; }
		public string CustomerName { get; set; }
		public string CustomerNumber { get; set; }
		public string ReasonCode { get; set; }
		public string Remarks { get; set; }
	}
}
