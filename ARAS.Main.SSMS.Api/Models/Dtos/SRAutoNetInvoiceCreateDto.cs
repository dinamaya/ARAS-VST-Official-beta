namespace ARAS.Main.SSMS.Api.Models.Dtos
{
	public class SRAutoNetInvoiceCreateDto : InvoiceCreateDto
	{
		public string ReasonCode { get; set; } = string.Empty;
		public IList<SRAutoNetRemarksDto> Remarks { get; set; }
	}
}
