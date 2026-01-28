namespace ARAS.Main.SSMS.Api.Models.Dtos
{
    public class SearchRequestDto
    {
        public string Category { get; set; }
        public string Value { get; set; }
		public DateOnly StartDate { get; set; }
		public DateOnly EndDate { get; set; }
	}
}
