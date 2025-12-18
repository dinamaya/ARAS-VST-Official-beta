namespace ARAS.Blazor.Models.DTOs
{
	public class SRAutoNetCreateDto
	{
		public long InvoiceId { get; set; }
		public long RequestId { get; set; }
		public double InvoiceAmount { get; set; }
		public DateTime InvoiceDate { get; set; }
		public string InvoiceNumber { get; set; }
		public string CustomerName { get; set; }
		public string CustomerNumber { get; set; }
		public string ReasonCode { get; set; } = string.Empty;
		public IList<SRAutoNetRemarksDto> Remarks { get; set; }
	}
}
