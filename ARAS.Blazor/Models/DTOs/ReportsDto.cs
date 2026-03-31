namespace ARAS.Blazor.Models.DTOs
{
    public class ReportsDto
    {
        public long RequestId { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string AdjustmentTypeCode { get; set; } = string.Empty;
        public string AdjustmentType { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string InvoiceNumber { get; set; } = string.Empty;
        public string RequestorId { get; set; } = string.Empty;
        public string RequestorSearchName { get; set; } = string.Empty;
        public string Requestor { get; set; } = string.Empty;
        public string Approver { get; set; } = string.Empty;
        public string UpdatedBy { get; set; } = string.Empty;
        public DateTime DateRequested { get; set; }
        public DateTime? DateApproved { get; set; }
        public DateTime DateUpdated { get; set; }
        public string Status { get; set; } = string.Empty;
        public double Amount { get; set; }
    }
}
