namespace ARAS.Main.SSMS.Api.Models.Dtos
{
	public class BaseaAdjustmentCreateValidationDto
	{
		public string InvoiceNumber { get; set; }
		public double AdjustmentAmount { get; set; }
		public string Remarks { get; set; }
	}
}
