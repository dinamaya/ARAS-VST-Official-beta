using ARAS.Main.SSMS.Api.Models.Dtos;

namespace ARAS.Main.SSMS.Api.Repositories.Interfaces
{
	public interface ICashDiscountRepository : IBaseAdjustmentCommandRepository<CashDiscountRowDto, CashDiscountCreateDto, CashDiscountCreateValidationDto>
	{
	}
}
