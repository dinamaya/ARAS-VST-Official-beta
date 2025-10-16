using ARAS.Blazor.App_Code.Globals.Extensions;
using ARAS.Main.SSMS.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.Repositories.Interfaces;
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
		private readonly ICashDiscountRepository _cashDiscountRepo;

		public DeclinesController(ILogger<DeclinesController> logger, ICashDiscountRepository cashDiscountRepo)
		{
			_logger = logger;
			_cashDiscountRepo = cashDiscountRepo;
		}

		[HttpPost("cdr/{requestId:long}")]
		public async Task<ResponseDto<string>> DeclineTransactionRequestByRequestId(long requestId)
		{
			var response = new ResponseDto<string>();
			try
			{
				string accountId = User.GetIdentityClaim(ClaimTypes.PrimarySid);
				await _cashDiscountRepo.CreateDeclineTransaction(requestId, accountId);
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
