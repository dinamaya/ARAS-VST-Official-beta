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
		private readonly ISmallAmountRepository _smallAmountRepo;
		private readonly ISRAutoNetRepository _srAutoNetRepo;
        private readonly IARInvoiceOffsettingRepository _arInvoiceOffsettingRepo;

        public AdjustmentsController(ILogger<AdjustmentsController> logger, ICashDiscountRepository cashDiscountRepo, IAPAROffsetRepository aparOffsetRepo, IBankChargeRepository bankChargeRepo, IARInvoiceOffsettingRepository arInvoiceOffsettingRepo, ISmallAmountRepository smallAmountRepo, ISRAutoNetRepository srAutoNetRepo)
        {
            _logger = logger;
            _cashDiscountRepo = cashDiscountRepo;
            _aparOffsetRepo = aparOffsetRepo;
			_bankChargeRepo = bankChargeRepo;
            _arInvoiceOffsettingRepo = arInvoiceOffsettingRepo;
			_smallAmountRepo = smallAmountRepo;
			_srAutoNetRepo = srAutoNetRepo;
        }

        [HttpGet("ofr/{requestId:long}"), Authorize]
        public async Task<ResponseDto<IEnumerable<ARInvoiceOffsettingRowDto>>> GetARInvoiceOffsettingAdjustments(long requestId)
        {
            var response = new ResponseDto<IEnumerable<ARInvoiceOffsettingRowDto>>();
            try
            {
                response.Result = await _arInvoiceOffsettingRepo.GetAdjustmentsByRequestId(requestId);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return response.Failed(ex.Message);
            }
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

		[HttpGet("aar/{requestId:long}")]
        public async Task<ResponseDto<APAROffsetRowDto>> GetAPAdjustments(long requestId)
        {
            var response = new ResponseDto<APAROffsetRowDto>();
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

		[HttpGet("sar/{requestId:long}"), Authorize]
		public async Task<ResponseDto<IEnumerable<SmallAmountRowDto>>> GeSmallAmountAdjustments(long requestId)
		{
			var response = new ResponseDto<IEnumerable<SmallAmountRowDto>>();
			try
			{
				response.Result = await _smallAmountRepo.GetAdjustmentsByRequestId(requestId);
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message);
				return response.Failed(ex.Message);
			}
		}

		[HttpGet("srr/{requestId:long}"), Authorize]
		public async Task<ResponseDto<IEnumerable<SRAutoNetRowDto>>> GetSRAutoNetAdjustments(long requestId)
		{
			var response = new ResponseDto<IEnumerable<SRAutoNetRowDto>>();
			try
			{
				response.Result = await _srAutoNetRepo.GetAdjustmentsByRequestId(requestId);
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
