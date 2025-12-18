using ARAS.Main.SSMS.Api.Models.Complex;

namespace ARAS.Main.SSMS.Api.Models.Dtos
{
	public class SRAutoNetRowDto : AdjustmentRow
	{
		public long InvoiceId { get; set; }
		public string ReasonCode { get; set; } = string.Empty;
		public  IList<SRAutoNetRemarksDto> Remarks { get; set; }

		public SRAutoNetRowDto() { }
	}
}
