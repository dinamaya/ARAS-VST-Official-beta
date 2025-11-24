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
	[Route("api/decline")]
	[ApiController]
	[Authorize]
	public class DeclinesController : ControllerBase
	{
		private readonly ILogger<DeclinesController> _logger;
		private readonly IAdjustmentService _adjustmentService;

		public DeclinesController(ILogger<DeclinesController> logger, IAdjustmentService adjustmentService)
		{
			_logger = logger;
			_adjustmentService = adjustmentService;
		}

		[HttpPost("{adjustmentTypeCode}")]
		public async Task<ResponseDto<string>> Post(string adjustmentTypeCode, [FromBody] NegateRequestDto createDecline)
		{
			var response = new ResponseDto<string>();
			try
			{
				string accountId = User.GetIdentityClaim(ClaimTypes.PrimarySid);
				await _adjustmentService.Decline(createDecline, accountId, adjustmentTypeCode);

				response.Result = "Success";
				response.Message = "Request Declined";
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
