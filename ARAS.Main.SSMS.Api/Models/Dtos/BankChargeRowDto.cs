using ARAS.Main.SSMS.Api.App_Code.Globals.Extensions;
using ARAS.Main.SSMS.Api.Models.Complex;

namespace ARAS.Main.SSMS.Api.Models.Dtos
{
	public class BankChargeRowDto : AdjustmentRow
	{
		public BankChargeRowDto() { }
		public BankChargeRowDto(double adjustmentAmount, string remarks) => SetValues((float) adjustmentAmount, remarks);

		public virtual void SetValues(float adjustmentAmount, string remarks)
		{
			AdjustmentAmount = adjustmentAmount;
			Remarks = string.IsNullOrEmpty(remarks) ? "Added " + adjustmentAmount.ToPhp() + " Bank Charge." : remarks;
		}
	}
}
