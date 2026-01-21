namespace ARAS.Main.SSMS.Api.Models.Dtos
{
    public class ARInvoiceOffsettingCreateDto : InvoiceCreateDto
    {
        public IList<ARInvoiceOffsettingCNDto> Remarks { get; set; }
    }
}
