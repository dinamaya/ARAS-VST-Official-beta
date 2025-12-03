using ARAS.Blazor.App_Code.Globals.Extensions;
using ARAS.Main.SSMS.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ARAS.Main.SSMS.Api.Controllers
{
	[Route("api/write-off")]
	[ApiController]
	public class WriteOffController : ControllerBase
	{
		private readonly ILogger<WriteOffController> _logger;
		private readonly ISmallAmountRepository _smallAmountRepo;
		private readonly IRequestRepository _requestRepo;

		public WriteOffController(ILogger<WriteOffController> logger, ISmallAmountRepository smallAmountRepo, IRequestRepository requestRepo)
		{
			_logger = logger;
			_requestRepo = requestRepo;
			_smallAmountRepo = smallAmountRepo;
		}

		[HttpPost("sar"), Authorize(Roles = "Requestor")]
		public async Task<ResponseDto<long>> Create([FromBody] AdjustmentRequestCreationDto<SmallAmountCreateDto> data)
		{
			ResponseDto<long> response = new();
			try
			{
				var accountInfo = User.GetAccountBasicInfo();

				var requestCreation = new RequestCreationDto<AdjustmentRequestCreationDto<SmallAmountCreateDto>>(data, accountInfo.GroupCode, accountInfo.FullName);

				long requestId = await _smallAmountRepo.CreateAsync(requestCreation, accountInfo.Id);

				response.Result = requestId;
				response.Message = "Request Created Successfully";
				return response;
			}
			catch (Exception ex)
			{
				return response.Failed(ex.Message);
			}
		}

		[HttpPost("sar/update/{requestId:long}"), Authorize(Roles = "Requestor")]
		public async Task<ResponseDto<string>> Update(long requestId, [FromBody] AdjustmentRequestCreationDto<SmallAmountCreateDto> data)
		{
			ResponseDto<string> response = new ResponseDto<string>();
			try
			{
				var accountInfo = User.GetAccountBasicInfo();
				var requestCreation = new RequestCreationDto<AdjustmentRequestCreationDto<SmallAmountCreateDto>>(data, accountInfo.GroupCode, accountInfo.FullName);

				await _smallAmountRepo.UpdateAsync(requestId, requestCreation, accountInfo.Id);

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
