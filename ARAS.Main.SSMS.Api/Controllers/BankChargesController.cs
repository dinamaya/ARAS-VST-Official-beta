using ARAS.Blazor.App_Code.Globals.Extensions;
using ARAS.Main.SSMS.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ARAS.Main.SSMS.Api.Controllers
{
	[Route("api/bank-charge")]
	[ApiController]
	public class BankChargesController : ControllerBase
	{
		private readonly ILogger<BankChargesController> _logger;
		private readonly IBankChargeRepository _bankChargeRepo;
		private readonly IRequestRepository _requestRepo;

		public BankChargesController(ILogger<BankChargesController> logger, IBankChargeRepository bankChargeRepo, IRequestRepository requestRepo)
		{
			_logger = logger;
			_bankChargeRepo = bankChargeRepo;
			_requestRepo = requestRepo;
		}

		[HttpPost, Authorize(Roles = "Requestor")]
		public async Task<ResponseDto<long>> Create([FromBody] AdjustmentRequestCreationDto<BankChargeCreateDto> data)
		{
			ResponseDto<long> response = new();
			try
			{
				var accountInfo = User.GetAccountBasicInfo();

				var requestCreation = new RequestCreationDto<AdjustmentRequestCreationDto<BankChargeCreateDto>>(data, accountInfo.GroupCode, accountInfo.FullName);

				long requestId = await _bankChargeRepo.CreateAsync(requestCreation, accountInfo.Id);

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
		public async Task<ResponseDto<string>> Update(long requestId, [FromBody] AdjustmentRequestCreationDto<BankChargeCreateDto> data)
		{
			ResponseDto<string> response = new ResponseDto<string>();
			try
			{
				var accountInfo = User.GetAccountBasicInfo();
				var requestCreation = new RequestCreationDto<AdjustmentRequestCreationDto<BankChargeCreateDto>>(data, accountInfo.GroupCode, accountInfo.FullName);

				await _bankChargeRepo.UpdateAsync(requestId, requestCreation, accountInfo.Id);

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
