using ARAS.Blazor.App_Code.Globals.Extensions;
using ARAS.Main.SSMS.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.Repositories.Interfaces;
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
		private readonly ICashDiscountRepository _cashDiscountRepo;

		public RejectionsController(ILogger<RejectionsController> logger, ICashDiscountRepository cashDiscountRepo)
		{
			_logger = logger;
			_cashDiscountRepo = cashDiscountRepo;
		}

		[HttpPost("cdr/{requestId:long}")]
		public async Task<ResponseDto<string>> RejectTransactionRequestByRequestId(long requestId)
		{
			var response = new ResponseDto<string>();
			try
			{
				string accountId = User.GetIdentityClaim(ClaimTypes.PrimarySid);
				await _cashDiscountRepo.CreateRejectTransaction(requestId, accountId);
				response.Result = "Success";
				response.Message = "Request Rejected";
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
