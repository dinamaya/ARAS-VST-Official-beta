namespace ARAS.Main.SSMS.Api.Models.Dtos
{
    public class ARInvoiceOffsettingCreateDto
    {
        public double InvoiceAmount { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string InvoiceNumber { get; set; }
        public string CustomerName { get; set; }
        public string CustomerNumber { get; set; }
        public string Remarks { get; set; }
    }
}
