using ARAS.Blazor.Models.Complex;

namespace ARAS.Blazor.Models.DTOs
{
    public class BaseReceiptAdjustmentCreateDto
    {
		public string AdjustmentType { get; set; }
		public double InvoiceAmount { get; set; }
		public double AdjustmentAmount { get; set; }
		public DateTime InvoiceDate { get; set; }
		public string InvoiceNumber { get; set; }
		public string CustomerName { get; set; }
		public string CustomerNumber { get; set; }
		public string Remarks { get; set; }

		public BaseReceiptAdjustmentCreateDto() { }
		public BaseReceiptAdjustmentCreateDto(string adjustmentTypeName, InvoiceDetailsDto invoiceDetails, ReceiptAdjustmentEntry adjumentEntry)
		{
			AdjustmentType = adjustmentTypeName;
			InvoiceAmount = invoiceDetails.InvoiceAmount;
			AdjustmentAmount = adjumentEntry.Amount;
			InvoiceDate = invoiceDetails.InvoiceDate;
			InvoiceNumber = invoiceDetails.InvoiceNumber;
			CustomerName = invoiceDetails.CustomerName;
			CustomerNumber = invoiceDetails.CustomerNumber;
			Remarks = adjumentEntry.Remarks;
		}
	}
}
