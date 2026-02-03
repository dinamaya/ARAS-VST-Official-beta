namespace ARAS.Main.Oracle.Api.Models.Dtos
{
    public class CustomerInvoiceRequestDto
    {
        public string InvoiceNumber { get; set; }
        public DateOnly InvoiceDate { get; set; }
        public string CustomerName { get; set; }
        public string CustomerNumber { get; set; }
	}
}
