using ARAS.Blazor.App_Code.Globals.Extensions;
using ARAS.Main.SSMS.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
		private readonly ICashDiscountRepository _cashDiscountRepo;

		public ApprovalsController(ILogger<ApprovalsController> logger, ICashDiscountRepository cashDiscountRepo)
		{
			_logger = logger;
			_cashDiscountRepo = cashDiscountRepo;
		}

		[HttpPost("cdr/{requestId:long}")]
		public async Task<ResponseDto<string>> ApproveTransactionRequestByRequestId(long requestId)
		{
			var response = new ResponseDto<string>();
			try
			{
				string accountId = User.GetIdentityClaim(ClaimTypes.PrimarySid);
				await _cashDiscountRepo.CreateApproveTransaction(requestId, accountId);
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
