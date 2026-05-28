namespace ARAS.Main.Oracle.Api.Models.Dtos
{
	public class InvoiceDetailsQueryDto
	{
		public string? TrxNumber { get; set; }
		public string? CustomerName { get; set; }
		public DateOnly? StartDate { get; set; }
		public DateOnly? EndDate { get; set; }
	}
}
