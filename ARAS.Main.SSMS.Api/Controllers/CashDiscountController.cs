using ARAS.Blazor.App_Code.Globals.Extensions;
using ARAS.Main.Oracle.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.Repositories.Interfaces;
using ARAS.Main.SSMS.Api.Services.Interfaces;
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

		//[HttpPost("create")]
		[HttpPost("create"), Authorize(Roles = "Requestor")]
		public async Task<ResponseDto<string>> Create([FromBody] AdjustmentRequestCreationDto<CashDiscountCreateDto> data)
		{
			ResponseDto<string> response = new ResponseDto<string>();
			try
			{
				var accountInfo = User.GetAccountBasicInfo();

				var requestCreation = new RequestCreationDto<AdjustmentRequestCreationDto<CashDiscountCreateDto>>(data, accountInfo.GroupCode, accountInfo.FullName);

				await _cashDiscountRepo.CreateAsync(requestCreation, accountInfo.Id);

				response.Result = "Success";
				response.Message = "Request Created Successfully";
				return response;
			}
			catch (Exception ex)
			{
				return response.Failed(ex.Message);
			}
		}

		//[HttpPost("update/{requestId:long}")]
		[HttpPost("update/{requestId:long}"), Authorize(Roles = "Requestor")]
		public async Task<ResponseDto<string>> Update(long requestId, [FromBody] AdjustmentRequestCreationDto<CashDiscountCreateDto> data)
		{
			ResponseDto<string> response = new ResponseDto<string>();
			try
			{
				var accountInfo = User.GetAccountBasicInfo();
				var requestCreation = new RequestCreationDto<AdjustmentRequestCreationDto<CashDiscountCreateDto>>(data, accountInfo.GroupCode, accountInfo.FullName);

				await _cashDiscountRepo.UpdateAsync(requestId, requestCreation, accountInfo.Id);

				response.Result = "Success";
				response.Message = "Request Updated Successfully";
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

		//[HttpGet("approvals")]
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

		//[HttpGet("validations")]
		[HttpGet("validations"), Authorize(Roles = "Validator")]
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
