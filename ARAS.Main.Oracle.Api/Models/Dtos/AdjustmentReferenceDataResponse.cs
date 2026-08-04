namespace ARAS.Main.Oracle.Api.Models.Dtos
{
	public sealed class AdjustmentReferenceDataResponse
	{
		public int StatusCode { get; set; }
		public string Message { get; set; }
		public List<AdjustmentReferenceDataItem> Data { get; set; } = [];
	}

	public sealed class AdjustmentReferenceDataItem
	{
		public long CustomerTrxId { get; set; }
		public long OrgId { get; set; }
		public long SetOfBooksId { get; set; }
		public long PaymentScheduleId { get; set; }
		public string TrxNumber { get; set; }
		public DateTime TrxDate { get; set; }
		public string CustomerName { get; set; }
		public string CustomerNumber { get; set; }
		public decimal AmountDueRemaining { get; set; }
	}
}
