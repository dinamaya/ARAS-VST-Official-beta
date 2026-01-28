namespace ARAS.Blazor.Models.DTOs
{
	public class TransactionHistoryDto
	{
		public long TransactionId { get; set; }
		public string RequestNumber { get; set; }
		public string Creator { get; set; }
		public string DateCreated { get; set; }
		public string Description { get; set; }
		public string AttachmentName { get; set; }
		public string Status { get; set; }
		public string AccountRole { get; set; }
	}
}
