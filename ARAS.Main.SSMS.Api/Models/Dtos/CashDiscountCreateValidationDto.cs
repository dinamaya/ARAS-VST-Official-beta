namespace ARAS.Main.SSMS.Api.Models.Dtos
{
	public class CashDiscountCreateValidationDto
	{
		public string InvoiceNumber { get; set; }
		public double DiscountPercentage { get; set; }
		public string Remarks { get; set; }
	}
}
