namespace ARAS.Blazor.Models.DTOs
{
    public class ReportsDto
    {
        public string RequestNumber { get; set; } = string.Empty;
        public string AdjustmentTypeCode { get; set; } = string.Empty;
        public DateTime DateRequested { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
