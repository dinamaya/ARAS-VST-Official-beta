using ARAS.Main.Oracle.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.Controllers;
using ARAS.Main.SSMS.Api.Models.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ARAS.Main.SSMS.Api.Repositories.Interfaces
{
	public interface ICashDiscountRepository : ICreateRepository<IEnumerable<CashDiscountCreateDto>>
	{
		Task<IEnumerable<TransactionRequestRowDto>> GetAllSubmissions();
		Task<IEnumerable<TransactionRequestRowDto>> GetAllForApprovals();
		Task<IEnumerable<TransactionRequestRowDto>> GetAllForValidations();
		Task<IEnumerable<CashDiscountRowDto>> GetAdjustmentsByRequestId(long requestId);
		Task CreateApproveTransaction(long requestId, string createdBy);
		Task CreateValidateTransaction(long requestId, string createdBy);
		Task<TransactionRequestRowDto> GetTransactionRequestByRequestId(long requestId);

	}
}
