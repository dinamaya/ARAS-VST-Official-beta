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
		public long? CustomerTrxId { get; set; }
		public long? PaymentScheduleId { get; set; }
		public long? ReceiptId { get; set; }
		public string Remarks { get; set; }
		public string SourceType { get; set; } = "Receipt";
		public bool ApplyReceiptEnabled { get; set; }
		public IList<ReceiptInvoiceApplicationSelectionDto> ReceiptApplications { get; set; } = [];

		public BaseReceiptAdjustmentCreateDto() { }
		public BaseReceiptAdjustmentCreateDto(
	string adjustmentTypeName,
	InvoiceDetailsDto invoiceDetails,
	ReceiptAdjustmentEntry adjustmentEntry,
	string sourceType)
{
	AdjustmentType = adjustmentTypeName;
	InvoiceAmount = invoiceDetails.ReceiptAmount ?? invoiceDetails.InvoiceAmount;
	AdjustmentAmount = adjustmentEntry.Amount;
	InvoiceDate = invoiceDetails.ReceiptDate ?? invoiceDetails.InvoiceDate;

	// The API still calls this field InvoiceNumber,
	// but receipt requests must send the ReceiptNumber.
	InvoiceNumber = !string.IsNullOrWhiteSpace(invoiceDetails.ReceiptNumber)
		? invoiceDetails.ReceiptNumber
		: invoiceDetails.InvoiceNumber;

	CustomerName = invoiceDetails.CustomerName;
	CustomerNumber = invoiceDetails.CustomerNumber;
	CustomerTrxId = invoiceDetails.CustomerTrxId;
	PaymentScheduleId = invoiceDetails.PaymentScheduleId;
	ReceiptId = invoiceDetails.ReceiptId;
	Remarks = adjustmentEntry.Remarks;
	SourceType = sourceType;
}
	}
}
