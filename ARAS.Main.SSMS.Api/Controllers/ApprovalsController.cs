using ARAS.Blazor.App_Code.Globals.Extensions;
using ARAS.Main.SSMS.Api.Models.Dtos;
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

		public ApprovalsController(ILogger<ApprovalsController> logger, IAdjustmentService adjustmentService)
		{
			_logger = logger;
			_adjustmentService = adjustmentService;
		}

		[HttpPost("{adjustmentTypeCode}")]
		public async Task<ResponseDto<string>> Post(string adjustmentTypeCode, [FromBody] RequestUpdateDto data)
		{
			var response = new ResponseDto<string>();
			try
			{
				string accountId = User.GetIdentityClaim(ClaimTypes.PrimarySid);
				await _adjustmentService.Approve(data, accountId, adjustmentTypeCode);

				response.Result = "Success";
				response.Message = "Request Approved";
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
