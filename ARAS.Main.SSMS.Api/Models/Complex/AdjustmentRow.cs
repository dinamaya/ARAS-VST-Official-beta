
namespace ARAS.Main.SSMS.Api.Models.Complex
{
	public class AdjustmentRow
	{
		public string Id { get; set; }
		public double AdjustmentAmount { get; set; }
		public string AdjustmentActivity { get; set; } = string.Empty;
		public double InvoiceAmount { get; set; } = 1_000.00d;
		public string InvoiceDate { get; set; } = string.Empty;
		public string InvoiceNumber { get; set; } = string.Empty;
		public string CustomerName { get; set; } = string.Empty;
		public string CustomerNumber { get; set; } = string.Empty;
		public string ReasonCode { get; set; } = string.Empty;
		public string Remarks { get; set; } = string.Empty;

		public virtual void SetValues(double adjustmentAmount, string remarks)
		{
			Remarks = remarks;
			AdjustmentAmount = adjustmentAmount;
		}
	}
}
