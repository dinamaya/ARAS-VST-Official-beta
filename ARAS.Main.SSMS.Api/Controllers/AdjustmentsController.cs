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
		private readonly IAPAROffsetRepository _aparOffsetRepo;

        public AdjustmentsController(ILogger<AdjustmentsController> logger, ICashDiscountRepository cashDiscountRepo, IAPAROffsetRepository aparOffsetRepo)
        {
            _logger = logger;
            _cashDiscountRepo = cashDiscountRepo;
            _aparOffsetRepo = aparOffsetRepo;
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

        //[HttpPost("cdr/validate")]
        [HttpPost("cdr/validate"), Authorize]
		public async Task<ResponseDto<bool>> ValidateCashDiscountAdjustments([FromBody] CashDiscountCreateValidationDto CashDiscountCreateValidation)
		{
			var response = new ResponseDto<bool>();
			try
			{
				response.Result = await _cashDiscountRepo.IsValid(CashDiscountCreateValidation);
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
