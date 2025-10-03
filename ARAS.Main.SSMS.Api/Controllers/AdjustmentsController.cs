using ARAS.Main.SSMS.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ARAS.Main.SSMS.Api.Controllers
{
	[Route("api/adjustments")]
	[ApiController]
	public class AdjustmentsController : ControllerBase
	{
		private readonly ILogger<AdjustmentsController> _logger;
		private readonly ICashDiscountRepository _cashDiscountRepo;

		public AdjustmentsController(ILogger<AdjustmentsController> logger, ICashDiscountRepository cashDiscountRepo)
		{
			_logger = logger;
			_cashDiscountRepo = cashDiscountRepo;
		}


		[HttpGet("cdr/{requestId:long}"), Authorize]
		public async Task<ResponseDto<IEnumerable<CashDiscountRowDto>>> GetCashDiscountAdjustments(long requestId)
		{
			var response = new ResponseDto<IEnumerable<CashDiscountRowDto>>();
			try
			{
				response.Result = await _cashDiscountRepo.GetAdjustmentsByRequestId(requestId);
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
