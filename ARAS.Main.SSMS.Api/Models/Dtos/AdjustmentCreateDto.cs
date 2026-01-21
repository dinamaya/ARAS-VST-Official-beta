using System.ComponentModel.DataAnnotations;

namespace ARAS.Main.SSMS.Api.Models.Dtos
{
	public class AdjustmentCreateDto
	{
		public long InvoiceId { get; set; }
		public long RequestId { get; set; }
		public double AdjustmentAmount { get; set; }
		public string Remarks { get; set; }

		public AdjustmentCreateDto(long invoiceId, long requestId, BaseAdjustmentCreateDto data)
		{
			InvoiceId = invoiceId;
			RequestId = requestId;
			AdjustmentAmount = data.AdjustmentAmount;
			Remarks = data.Remarks;
		}

		public AdjustmentCreateDto(long invoiceId, long requestId, BaseReceiptAdjustmentCreateDto data)
		{
			InvoiceId = invoiceId;
			RequestId = requestId;
			AdjustmentAmount = data.AdjustmentAmount;
			Remarks = data.Remarks;
		}
	}
}