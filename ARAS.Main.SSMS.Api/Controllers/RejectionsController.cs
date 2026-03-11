using ARAS.Blazor.App_Code.Globals.Extensions;
using ARAS.Main.SSMS.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.Repositories.Interfaces;
using ARAS.Main.SSMS.Api.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ARAS.Main.SSMS.Api.Controllers
{
	[Route("api/reject")]
	[ApiController]
	public class RejectionsController : ControllerBase
	{
		private readonly ILogger<RejectionsController> _logger;
		private readonly IAdjustmentService _adjustmentService;
		private readonly IRequestRepository _requestRepo;

        public RejectionsController(ILogger<RejectionsController> logger, IAdjustmentService adjustmentService, IRequestRepository requestRepo)
        {
            _logger = logger;
            _adjustmentService = adjustmentService;
            _requestRepo = requestRepo;
        }

        [HttpPost]
		public async Task<ResponseDto<string>> Post([FromBody] IEnumerable<long> requestIds)
		{
			var response = new ResponseDto<string>();
			try
			{
				string accountId = User.GetIdentityClaim(ClaimTypes.PrimarySid);
				await _adjustmentService.Reject(requestIds, accountId);

				response.Result = "Success";
				response.Message = "Request has been successfully REJECTED";
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message);
				return response.Failed(ex.Message);
			}
		}

		[HttpPost("{requestId:long}")]
		public async Task<ResponseDto<string>> Post(long requestId)
		{
			var response = new ResponseDto<string>();
			try
			{
				string accountId = User.GetIdentityClaim(ClaimTypes.PrimarySid);
				await _requestRepo.Reject(requestId, accountId);

				response.Result = "Success";
				response.Message = "Request has been successfully REJECTED";
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
