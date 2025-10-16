namespace ARAS.Main.SSMS.Api.Models.Dtos
{
	public class TransactionHistoryDto
	{
		public long TransactionId { get; set; }
		public string RequestNumber { get; set; }
		public string Creator { get; set; }
		public string DateCreated { get; set; }
		public string Status { get; set; }
	}
}
