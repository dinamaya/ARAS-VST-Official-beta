namespace ARAS.Blazor.Models.DTOs
{
	public class APAROffsetARRowDto
	{
		public string Id { get; set; }
		public ARRowType RowType { get; set; } = ARRowType.Invoice;
		public double InvoiceAmount { get; set; } = 1_000.00d;
		public string InvoiceNumber { get; set; } = string.Empty;
		public string CustomerName { get; set; } = string.Empty;
		public string CustomerNumber { get; set; } = string.Empty;
		public string? AdjustmentReason { get; set; } = null;
		public double Amount { get; set; } = 0.00d;
		public long? CustomerTrxId { get; set; }
		public long? PaymentScheduleId { get; set; }

		public APAROffsetARRowDto() { }

		public APAROffsetARRowDto(InvoiceDetailsDto details)
		{
			Id = details.Id;
			Amount = details.InvoiceBalance;
			InvoiceNumber = details.InvoiceNumber;
			InvoiceAmount = details.InvoiceAmount;
			CustomerName = details.CustomerName;
			CustomerNumber = details.CustomerNumber;
			CustomerTrxId = details.CustomerTrxId;
			PaymentScheduleId = details.PaymentScheduleId;
		}
	}


	public enum ARRowType
	{
		Invoice,
		Adjustment
	}
}

