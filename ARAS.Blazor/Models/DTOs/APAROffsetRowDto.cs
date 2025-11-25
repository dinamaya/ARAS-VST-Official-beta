namespace ARAS.Blazor.Models.DTOs
{
	public record APAROffsetRowDto
	{
		public IEnumerable<APAROffsetAPRowDto> APGroup { get; set; } = new List<APAROffsetAPRowDto>();
		public IEnumerable<APAROffsetARRowDto> ARGroup { get; set; } = new List<APAROffsetARRowDto>();
	}
}
