namespace ARAS.Main.SSMS.Api.Models.Dtos
{
	public class TransactionCreateDto(long requestId, string statusName)
	{
		public long RequestId { get; set; } = requestId;
		public string StatusName { get; set; } = statusName;
	}
}
