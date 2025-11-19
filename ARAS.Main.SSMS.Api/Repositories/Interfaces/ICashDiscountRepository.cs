using ARAS.Main.SSMS.Api.Controllers;
using ARAS.Main.SSMS.Api.Models.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ARAS.Main.SSMS.Api.Repositories.Interfaces
{
	public interface ICashDiscountRepository : ICreateStatusRepository, IGetTransactionRequestsRepository, ICreateRepository<RequestCreationDto<AdjustmentRequestCreationDto<CashDiscountCreateDto>>, long>
	{
		Task<IEnumerable<CashDiscountRowDto>> GetAdjustmentsByRequestId(long requestId);
		Task UpdateAsync(long requestId, RequestCreationDto<AdjustmentRequestCreationDto<CashDiscountCreateDto>> data, string modifiedBy);
		Task<TransactionRequestRowDto> GetTransactionRequestByRequestId(long requestId);
		Task<bool> IsValid(CashDiscountCreateValidationDto cashCreateValidationRequest);
	}
}
