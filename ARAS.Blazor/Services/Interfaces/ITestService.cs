using ARAS.Blazor.Models.DTOs;

namespace ARAS.Blazor.Services.Interfaces
{
	public interface ITestService
	{
		Task GenerateCashDiscountRows(IList<CashDiscountRowDto> Adjustments, IEnumerable<string> reasonCodes);
		Task GenerateBankChargeRows(IList<BankChargeRowDto> Adjustments, IEnumerable<string> reasonCodes);
	}
}
