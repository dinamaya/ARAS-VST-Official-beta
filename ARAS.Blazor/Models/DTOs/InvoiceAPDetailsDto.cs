namespace ARAS.Blazor.Models.DTOs
{
	public class InvoiceAPDetailsDto
	{
        public string? Id { get; set; }
        public double InvoiceAmount { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime? InvoiceDate { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerNumber { get; set; } = string.Empty;
    }
}
