using ARAS.Blazor.App_Code.Globals;
using ARAS.Blazor.Models.DTOs;

namespace ARAS.Blazor.Models.Complex
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

		public AdjustmentRow() { }

		public AdjustmentRow(InvoiceDetailsDto details) 
		{
			Id = Utils.Security.GenerateExtendedGuid("",1);
			InvoiceAmount = details.InvoiceAmount;
			InvoiceNumber = details.InvoiceNumber;
			InvoiceDate = details.InvoiceDate.ToString();
			CustomerName = details.CustomerName;
			CustomerNumber = details.CustomerNumber;
		}

		public virtual void SetValues(float adjustmentAmount, string remarks, string reasonCode)
		{
			Remarks = remarks;
			AdjustmentAmount = adjustmentAmount;
			ReasonCode = reasonCode;
		}
	}
}
