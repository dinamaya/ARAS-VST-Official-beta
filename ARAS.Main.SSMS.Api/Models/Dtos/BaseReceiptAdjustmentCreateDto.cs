
namespace ARAS.Main.SSMS.Api.Models.Dtos
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
	}
}
