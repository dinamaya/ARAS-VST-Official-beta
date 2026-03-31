namespace ARAS.Blazor.Models.DTOs
{
    public class ReportFiltersDto
    {
        public string ReportType { get; set; } = "All Requests";
        public DateOnly StartDate { get; set; } = DateOnly.FromDateTime(DateTime.Today.AddYears(-1));
        public DateOnly EndDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
        public string Status { get; set; } = string.Empty;
        public string AdjustmentType { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string RequestorName { get; set; } = string.Empty;
    }
}
