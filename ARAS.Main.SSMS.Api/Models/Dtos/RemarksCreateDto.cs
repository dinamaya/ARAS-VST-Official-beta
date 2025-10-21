namespace ARAS.Main.SSMS.Api.Models.Dtos
{
	public class RemarksCreateDto
	{
		public RemarksCreateDto(long transactionId, string remarks)
		{
			TransactionId = transactionId;
			Remarks = remarks;
		}

		public long TransactionId { get; set; }
		public string Remarks { get; set; }
	}
}
