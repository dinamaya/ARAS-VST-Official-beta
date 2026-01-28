namespace ARAS.Main.SSMS.Api.Models.SQLVIews;

public partial class RequestsNumberSourceV
{
	public long RequestId { get; set; }

	public DateOnly? RequestDate { get; set; }

	public string? GroupCode { get; set; }

	public string AdjustmentTypeCode { get; set; } = null!;
}
