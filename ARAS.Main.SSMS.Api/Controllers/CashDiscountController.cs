using ARAS.Blazor.App_Code.Globals.Extensions;
using ARAS.Main.Oracle.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ARAS.Main.SSMS.Api.Controllers
{
	[Route("api/cash-discount")]
	[ApiController]
	public class CashDiscountController : ControllerBase
	{
		private readonly ILogger<CashDiscountController> _logger;
		private readonly ICashDiscountRepository _cashDiscountRepo;

		public CashDiscountController(ILogger<CashDiscountController> logger, ICashDiscountRepository cashDiscountRepo)
		{
			_logger = logger;
			_cashDiscountRepo = cashDiscountRepo;
		}

		[HttpPost, Authorize( Roles = "Requestor")]
		public async Task<ResponseDto<string>> Post([FromBody] IEnumerable<CashDiscountCreateDto> data)
		{
			ResponseDto<string> response = new ResponseDto<string>();
			try
			{
				string accountId = User.GetIdentityClaim(ClaimTypes.PrimarySid);
				response.Result = "Success";
				response.Message = "Request Created Successfully";
				await _cashDiscountRepo.CreateAsync(data, accountId);
				return response;
			}
			catch (Exception ex)
			{
				return response.Failed(ex.Message);
			}
		}

		[HttpGet("submissions"), Authorize]
		public async Task<ResponseDto<IEnumerable<TransactionRequestRowDto>>> GetAllSubmissions()
		{
			var response = new ResponseDto<IEnumerable<TransactionRequestRowDto>>();
			try
			{
				response.Message = "";
				response.Result = await _cashDiscountRepo.GetAllSubmissions();
				return response;
			}
			catch (Exception ex)
			{
				return response.Failed(ex.Message);
			}
		}

		[HttpGet("approvals"), Authorize(Roles = "Approver")]
		public async Task<ResponseDto<IEnumerable<TransactionRequestRowDto>>> GetForApprovals()
		{
			var response = new ResponseDto<IEnumerable<TransactionRequestRowDto>>();
			try
			{
				response.Result = await _cashDiscountRepo.GetAllForApprovals();
				return response;
			}
			catch (Exception ex)
			{
				return response.Failed(ex.Message);
			}
		}

		[HttpGet("validations")]
		//[Authorize(Roles = "Validator")]
		public async Task<ResponseDto<IEnumerable<TransactionRequestRowDto>>> GetForValidations()
		{
			var response = new ResponseDto<IEnumerable<TransactionRequestRowDto>>();
			try
			{
				response.Result = await _cashDiscountRepo.GetAllForValidations();
				return response;
			}
			catch (Exception ex)
			{
				return response.Failed(ex.Message);
			}
		}
	}
}
