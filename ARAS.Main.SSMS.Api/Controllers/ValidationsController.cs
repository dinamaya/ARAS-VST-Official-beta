using ARAS.Blazor.App_Code.Globals.Extensions;
using ARAS.Main.SSMS.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.Repositories.Interfaces;
using ARAS.Main.SSMS.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ARAS.Main.SSMS.Api.Controllers
{
	[Route("api/validate")]
	[ApiController]
	public class ValidationsController : ControllerBase
	{
		private readonly ILogger<ValidationsController> _logger;
		private readonly IAdjustmentService _adjustmentService;
		private readonly ICashDiscountRepository _cashDiscountRepo;

		public ValidationsController(ILogger<ValidationsController> logger, IAdjustmentService adjustmentService, ICashDiscountRepository cashDiscountRepo)
		{
			_logger = logger;
			_adjustmentService = adjustmentService;
			_cashDiscountRepo = cashDiscountRepo;
		}


		[HttpPost("{adjustmentTypeCode}")]
		[Authorize(Roles = "Validator")]
		public async Task<ResponseDto<string>> Post(string adjustmentTypeCode, [FromBody] RequestUpdateDto data)
		{
			var response = new ResponseDto<string>();
			try
			{
				string accountId = User.GetIdentityClaim(ClaimTypes.PrimarySid);
				await _adjustmentService.Validate(data, accountId, adjustmentTypeCode);
		
				response.Result = "Success";
				response.Message = "Request Validated";
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message);
				return response.Failed(ex.Message);
			}
		}

		[HttpPost("input/cdr"), Authorize]
		public async Task<ResponseDto<bool>> ValidateCashDiscountAdjustments([FromBody] CashDiscountCreateValidationDto CashDiscountCreateValidation)
		{
			var response = new ResponseDto<bool>();
			try
			{
				response.Result = await _cashDiscountRepo.IsValid(CashDiscountCreateValidation);
				response.Message = response.Result ? "Invoice Number is valid for Cash Discount" : "Invoice Number has already a cash discount adjustment";
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message);
				return response.Failed(ex.Message);
			}
		}
	}
}
