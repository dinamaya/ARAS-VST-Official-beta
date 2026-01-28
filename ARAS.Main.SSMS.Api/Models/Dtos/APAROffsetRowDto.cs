namespace ARAS.Main.SSMS.Api.Models.Dtos
{
	public record APAROffsetRowDto
	{
		public IEnumerable<APAROffsetAPRowDto> APGroup { get; set; }
		public IEnumerable<APAROffsetARRowDto> ARGroup { get; set; }
	}
}
