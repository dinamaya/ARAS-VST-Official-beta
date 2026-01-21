namespace ARAS.Blazor.Models.DTOs
{
    public class ARInvoiceOffsettingCNDetailsDto
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string CNRef { get; set; } = string.Empty;
        public double CNAmt { get; set; } = 0.00d;
        public double WT { get; set; } = 0.00d;

        public DateTime CNDate { get; set; } // Invoice Amount
        public string CustomerName { get; set; }
        public string CustomerNumber { get; set; }
        public string? Remarks { get; set; } = string.Empty;

        public bool IsEmpty() => string.IsNullOrEmpty(CNRef) && CNAmt <= 0 && WT <= 0;
        public bool HasEmpty() => string.IsNullOrEmpty(CNRef) || CNAmt <= 0 || WT <= 0;
    }
}
