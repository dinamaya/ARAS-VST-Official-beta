using ARAS.Blazor.App_Code.Globals.Constants;
using ARAS.Blazor.Models.Complex;

namespace ARAS.Blazor.Models.DTOs
{
	public class SRAutoNetRowDto : AdjustmentRow
	{
		public string GLDate { get; set; } // OR Invoice Date
		public string ReasonCode { get; set; } = string.Empty;
		public  IList<SRAutoNetRemarksDto> Remarks { get; set; }

		public SRAutoNetRowDto() { }

		public SRAutoNetRowDto(string reasonCode, IList<SRAutoNetRemarksDto> remarks, InvoiceDetailsDto details) : base(details)
		{
			SetValues(reasonCode, remarks, details);
		}

		public void SetValues(string reasonCode, IList<SRAutoNetRemarksDto> remarks, InvoiceDetailsDto details)
		{
			Remarks = remarks;
			ReasonCode = reasonCode;

			AdjustmentAmount = remarks.Sum(r => r.WT);
			AdjustmentActivity = "Offset to Other Income/Expense";

			GLDate = details.InvoiceDate.ToString(Formats.Date.DISPLAY3);
		}
	}
}
