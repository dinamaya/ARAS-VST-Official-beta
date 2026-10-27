namespace ARAS.Main.SSMS.Api.Models.Dtos
{
    public class BaseAdjustmentCreateDto
    {
		public double InvoiceAmount { get; set; }
		public double InvoiceBalance { get; set; }
		public double AdjustmentAmount { get; set; }
		public DateTime InvoiceDate { get; set; }
		public string InvoiceNumber { get; set; }
		public string CustomerName { get; set; }
		public string CustomerNumber { get; set; }
		public long? CustomerTrxId { get; set; }
		public long? PaymentScheduleId { get; set; }
		public string ReasonCode { get; set; }
		public string Remarks { get; set; }
	}
}
