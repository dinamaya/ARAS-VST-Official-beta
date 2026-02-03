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
		private readonly IAdjustmentRepository _adjustmentRepo;
		private readonly IAPAROffsetRepository _aparOffsetRepo;
		private readonly IAROffsettingRepository _arOffsetRepo;
		private readonly IBaseReceiptAdjustmentRepository _receiptRepo;

        public AdjustmentsController(IAdjustmentRepository adjustmentRepo, IAPAROffsetRepository aparOffsetRepo, IAROffsettingRepository arOffsetRepo, IBaseReceiptAdjustmentRepository receiptRepo)
        {
            _adjustmentRepo = adjustmentRepo;
            _aparOffsetRepo = aparOffsetRepo;
            _arOffsetRepo = arOffsetRepo;
            _receiptRepo = receiptRepo;
        }

        [HttpGet("activity/{adjustmentTypeCode}")]
		public async Task<ResponseDto<string>> GetAdjustmentActivity(string adjustmentTypeCode)
		{
			var response = new ResponseDto<string>();
			try
			{
				response.Result = await _adjustmentRepo.GetActivityNameByCode(adjustmentTypeCode);
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message);
				return response.Failed(ex.Message);
			}
		}

		[HttpGet("types")]
		public async Task<ResponseDto<IEnumerable<string>>> GetAdjustmentTypes()
		{
			var response = new ResponseDto<IEnumerable<string>>();
			try
			{
				response.Result = await _adjustmentRepo.GetTypes();
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message);
				return response.Failed(ex.Message);
			}
		}

		[HttpGet("types/receipt")]
		public async Task<ResponseDto<IEnumerable<string>>> GetReceiptAdjustmentTypes()
		{
			var response = new ResponseDto<IEnumerable<string>>();
			try
			{
				response.Result = await _adjustmentRepo.GetReceiptTypes();
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message);
				return response.Failed(ex.Message);
			}
		}

		[HttpGet("types/invoice")]
		public async Task<ResponseDto<IEnumerable<string>>> GetInvoiceAdjustmentTypes()
		{
			var response = new ResponseDto<IEnumerable<string>>();
			try
			{
				response.Result = await _adjustmentRepo.GetInvoiceTypes();
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

		[HttpGet("ofr/{requestId:long}")]
		public async Task<ResponseDto<IEnumerable<ARInvoiceOffsettingRowDto>>> GetARAdjustments(long requestId)
		{
			var response = new ResponseDto<IEnumerable<ARInvoiceOffsettingRowDto>>();
			try
			{
				response.Result = await _arOffsetRepo.GetAdjustmentsByRequestId(requestId);
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message);
				return response.Failed(ex.Message);
			}
		}

		[HttpGet("stage/ofr/{requestId:long}")]
		public async Task<ResponseDto<IEnumerable<AdjustmentPostingDto>>> GetARInvoiceStagingData(long requestId)
		{
			var response = new ResponseDto<IEnumerable<AdjustmentPostingDto>>();
			try
			{
				response.Result = await _arOffsetRepo.GetStagingData(requestId);
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message);
				return response.Failed(ex.Message);
			}
		}


		[HttpGet("stage/receipt/{requestId:long}")]
		public async Task<ResponseDto<IEnumerable<AdjustmentPostingDto>>> GetReceiptAdjustmentStagingData(long requestId)
		{
			var response = new ResponseDto<IEnumerable<AdjustmentPostingDto>>();
			try
			{
				response.Result = await _receiptRepo.GetStagingData(requestId);
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
