namespace ARAS.Blazor.Models.DTOs
{
    public class APAROffsetCreateDto
    {
        public string InvoiceId { get; set; }
        public double Amount { get; set; }
        public string Type { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string CustomerName { get; set; }
        public string CustomerNumber { get; set; }
    }
}
