namespace ARAS.Main.SSMS.Api.Models.Dtos
{
	public class RequestCreateDto(string? requestNumber, string? adjustmentTypeId)
	{
		public string? RequestNumber { get; set; } = requestNumber;
		public string? AdjustmentTypeId { get; set; } = adjustmentTypeId;
	}
}
