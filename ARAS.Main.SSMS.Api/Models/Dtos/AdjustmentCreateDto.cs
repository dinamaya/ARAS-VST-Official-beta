using System.ComponentModel.DataAnnotations;

namespace ARAS.Main.SSMS.Api.Models.Dtos
{
	public class AdjustmentCreateDto
	{
		public long InvoiceId { get; set; }
		public long RequestId { get; set; }
		public double AdjustmentAmount { get; set; }
		public string AdjustmentTypeId { get; set; }
		public string Remarks { get; set; }
		public string ReasonCode { get; set; }

		public AdjustmentCreateDto(long invoiceId, long requestId, double adjustmentAmount, string adjustmentTypeId, string remarks, string reasonCode)
		{
			InvoiceId = invoiceId;
			RequestId = requestId;
			AdjustmentAmount = adjustmentAmount;
			AdjustmentTypeId = adjustmentTypeId;
			Remarks = remarks;
			ReasonCode = reasonCode;
		}

		public AdjustmentCreateDto(long invoiceId, long requestId, BaseAdjustmentCreateDto data, string adjustmentTypeId)
		{
			InvoiceId = invoiceId;
			RequestId = requestId;
			AdjustmentAmount = data.AdjustmentAmount;
			AdjustmentTypeId = adjustmentTypeId;
			Remarks = data.Remarks;
			ReasonCode = data.ReasonCode;
		}
	}
}