namespace ARAS.Blazor.Models.DTOs
{
    public class SearchRequestDto
    {
        public string Category { get; set; }
        public string Value { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
    }
}
