using ARAS.Blazor.Models.Complex;
using ARAS.Blazor.Models.DTOs;

namespace ARAS.Blazor.Services.Interfaces
{
	public interface ITestService
	{
		Task GenerateAdjustmentRows(IList<CashDiscountRowDto> Adjustments, IEnumerable<string> reasonCodes, string adjustmentActivity);
		Task GenerateAdjustmentRows(IList<BankChargeRowDto> Adjustments, IEnumerable<string> reasonCodes, string adjustmentActivity);
		Task GenerateAdjustmentRows(IList<SmallAmountRowDto> Adjustments, IEnumerable<string> reasonCodes, string adjustmentActivity);
		Task GenerateAdjustmentRows(IList<SRAutoNetRowDto> Adjustments, IEnumerable<string> reasonCodes, string adjustmentActivity);
		Task GenerateAdjustmentRows(IList<APAROffsetAPRowDto> apGroup, IList<APAROffsetARRowDto> arGroup, IEnumerable<string> reasonCodes, string adjustmentActivity);
	}
}
