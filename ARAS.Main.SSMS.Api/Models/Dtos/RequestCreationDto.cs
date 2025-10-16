namespace ARAS.Main.SSMS.Api.Models.Dtos
{
	public class RequestCreationDto<TAdjustment>(IEnumerable<TAdjustment> adjustments, string groupCode)
	{
		public IEnumerable<TAdjustment> Adjustments { get; private set; } = adjustments;
		public string GroupCode { get; private set; } = groupCode;
	}
}
