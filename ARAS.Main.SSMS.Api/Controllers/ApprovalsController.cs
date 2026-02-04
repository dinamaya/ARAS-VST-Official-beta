using ARAS.Blazor.App_Code.Globals.Extensions;
using ARAS.Main.SSMS.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.Repositories.Interfaces;
using ARAS.Main.SSMS.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ARAS.Main.SSMS.Api.Controllers
{
	[Route("api/approve")]
	[ApiController]
	[Authorize(Roles = "Approver")]
	public class ApprovalsController : ControllerBase
	{
		private readonly ILogger<ApprovalsController> _logger;
		private readonly IAdjustmentService _adjustmentService;
		private readonly IAdjustmentRepository _adjustmentRepo;
		private readonly IRequestRepository _requestRepo;

        public ApprovalsController(ILogger<ApprovalsController> logger, IAdjustmentService adjustmentService, IRequestRepository requestRepo, IAdjustmentRepository adjustmentRepo)
        {
            _logger = logger;
            _adjustmentService = adjustmentService;
            _requestRepo = requestRepo;
            _adjustmentRepo = adjustmentRepo;
        }

        [HttpPost]
		public async Task<ResponseDto<string>> Post([FromBody] IEnumerable<long> requestIds)
		{
			var response = new ResponseDto<string>();
			try
			{
				string accountId = User.GetIdentityClaim(ClaimTypes.PrimarySid);
				await _adjustmentService.Approve(requestIds, accountId);

				response.Result = "Success";
				response.Message = "Request has been successfully APPROVED";
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
				await _requestRepo.Approve(requestId, accountId);

				response.Result = "Success";
				response.Message = "Request has been successfully APPROVED";
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message);
				return response.Failed(ex.Message);
			}
		}

		[HttpGet, AllowAnonymous]
		public async Task<ResponseDto<IEnumerable<long>>> Get()
		{
			var response = new ResponseDto<IEnumerable<long>>();
			try
			{
				response.Result = await _adjustmentRepo.GetAllApproved();
				response.Message = "Successfully Fetched approved request adjustment IDs";
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
