namespace ARAS.Blazor.Models.DTOs
{
    public class APAROffsetAPRowDto
    {
        public string Id { get; set; }
        public double InvoiceAmount { get; set; } = 1_000.00d;
        public string InvoiceNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerNumber { get; set; } = string.Empty;

        public APAROffsetAPRowDto(InvoiceDetailsDto details)
        {
            Id = details.Id;
            InvoiceAmount = details.InvoiceAmount;
            InvoiceNumber = details.InvoiceNumber;
            CustomerName = details.CustomerName;
            CustomerNumber = details.CustomerNumber;
        }
    }
}

