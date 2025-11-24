using ARAS.Blazor.App_Code.Globals.Extensions;
using ARAS.Main.SSMS.Api.Models.Dtos;
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
		private readonly IRequestRepository _requestRepo;

		public CashDiscountController(ILogger<CashDiscountController> logger, ICashDiscountRepository cashDiscountRepo, IRequestRepository requestRepo)
		{
			_logger = logger;
			_cashDiscountRepo = cashDiscountRepo;
			_requestRepo = requestRepo;
		}

		[HttpPost, Authorize(Roles = "Requestor")]
		public async Task<ResponseDto<long>> Create([FromBody] AdjustmentRequestCreationDto<CashDiscountCreateDto> data)
		{
			ResponseDto<long> response = new ();
			try
			{
				var accountInfo = User.GetAccountBasicInfo();

				var requestCreation = new RequestCreationDto<AdjustmentRequestCreationDto<CashDiscountCreateDto>>(data, accountInfo.GroupCode, accountInfo.FullName);

				long requestId = await _cashDiscountRepo.CreateAsync(requestCreation, accountInfo.Id);

				response.Result = requestId;
				response.Message = "Request Created Successfully";
				return response;
			}
			catch (Exception ex)
			{
				return response.Failed(ex.Message);
			}
		}

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
	}
}
