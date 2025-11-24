using ARAS.Main.SSMS.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ARAS.Main.SSMS.Api.Controllers
{
	[Route("api/adjustments")]
	[ApiController]
	public class AdjustmentsController : ControllerBase
	{
		private readonly ILogger<AdjustmentsController> _logger;
		private readonly ICashDiscountRepository _cashDiscountRepo;
		private readonly IBankChargeRepository _bankChargeRepo;
		private readonly IAPAROffsetRepository _aparOffsetRepo;

        public AdjustmentsController(ILogger<AdjustmentsController> logger, ICashDiscountRepository cashDiscountRepo, IAPAROffsetRepository aparOffsetRepo, IBankChargeRepository bankChargeRepo)
        {
            _logger = logger;
            _cashDiscountRepo = cashDiscountRepo;
            _aparOffsetRepo = aparOffsetRepo;
			_bankChargeRepo = bankChargeRepo;
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

		[HttpGet("bca/{requestId:long}"), Authorize]
		public async Task<ResponseDto<IEnumerable<BankChargeRowDto>>> GetBankChargeAdjustments(long requestId)
		{
			var response = new ResponseDto<IEnumerable<BankChargeRowDto>>();
			try
			{
				response.Result = await _bankChargeRepo.GetAdjustmentsByRequestId(requestId);
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message);
				return response.Failed(ex.Message);
			}
		}

		[HttpGet("aar/{requestId:long}"), Authorize]
        public async Task<ResponseDto<Tuple<IEnumerable<APAROffsetAPRowDto>, IEnumerable<APAROffsetARRowDto>>>> GetAPAdjustments(long requestId)
        {
            var response = new ResponseDto<Tuple<IEnumerable<APAROffsetAPRowDto>, IEnumerable<APAROffsetARRowDto>>>();
            try
            {
                response.Result = await _aparOffsetRepo.GetAPAdjustmentsByRequestId(requestId);
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
