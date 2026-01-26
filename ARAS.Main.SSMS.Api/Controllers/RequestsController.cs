using ARAS.Blazor.App_Code.Globals.Extensions;
using ARAS.Main.SSMS.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ARAS.Main.SSMS.Api.Controllers
{
	[Route("api/requests")]
	[ApiController]
	public class RequestsController : ControllerBase
	{
		private readonly ILogger<RequestsController> _logger;
		private readonly IRequestRepository _requestRepo;
		private readonly IBaseReceiptAdjustmentRepository _receiptAdjustmentRepo;
		private readonly IAPAROffsetRepository _aparRepo;
		private readonly IAROffsettingRepository _arRepo;

        public RequestsController(ILogger<RequestsController> logger, IRequestRepository requestRepo, IBaseReceiptAdjustmentRepository receiptAdjustmentRepo, IAPAROffsetRepository aparRepo, IAROffsettingRepository arRepo)
        {
            _logger = logger;
            _requestRepo = requestRepo;
            _receiptAdjustmentRepo = receiptAdjustmentRepo;
            _aparRepo = aparRepo;
            _arRepo = arRepo;
        }

        [HttpGet("details/{requestId:long}"), Authorize]
		public async Task<ResponseDto<TransactionRequestRowDto>> GetTransactionRequestByRequestId(long requestId)
		{
			var response = new ResponseDto<TransactionRequestRowDto>();
			try
			{
				response.Result = await _requestRepo.GetTransactionRequestByRequestId(requestId);
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message);
				return response.Failed(ex.Message);
			}
		}

		[HttpGet("is-approvable/{requestId:long}"), Authorize(Roles = "Approver")]
		public async Task<ResponseDto<bool>> IsApprovable(long requestId)
		{
			var response = new ResponseDto<bool>();
			try
			{
				response.Result = await _requestRepo.IsApprovable(requestId);
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message);
				return response.Failed(ex.Message);
			}
		}

		[HttpGet("is-declinable/{requestId:long}"), Authorize]
		public async Task<ResponseDto<bool>> IsDeclinable(long requestId)
		{
			var response = new ResponseDto<bool>();
			try
			{
				response.Result = await _requestRepo.IsDeclinable(requestId);
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message);
				return response.Failed(ex.Message);
			}
		}

		[HttpGet("is-rejectable/{requestId:long}"), Authorize]
		public async Task<ResponseDto<bool>> IsRejectable(long requestId)
		{
			var response = new ResponseDto<bool>();
			try
			{
				response.Result = await _requestRepo.IsRejectable(requestId);
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message);
				return response.Failed(ex.Message);
			}
		}

		[HttpGet("is-declined/{requestId:long}"), Authorize]
		public async Task<ResponseDto<bool>> IsDeclined(long requestId)
		{
			var response = new ResponseDto<bool>();
			try
			{
				response.Result = await _requestRepo.IsDeclined(requestId);
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message);
				return response.Failed(ex.Message);
			}
		}

		[HttpPost("receipt"), Authorize(Roles = "Requestor")]
		public async Task<ResponseDto<string>> CreateReceiptAdjusmentRequest([FromBody] IEnumerable<BaseReceiptAdjustmentCreateDto> data)
		{
			var response = new ResponseDto<string>();
			try
			{
				var requestCreation = new RequestCreationDto<IEnumerable<BaseReceiptAdjustmentCreateDto>>(data, User.GetAccountBasicInfo());
				response.Result = await _receiptAdjustmentRepo.Create(requestCreation, requestCreation.CreatorId);
				return response;
			}
			catch (Exception ex)
			{
				return response.Failed(ex.Message);
			}
		}

		[HttpPost("invoice/arr"), Authorize(Roles = "Requestor")]
		public async Task<ResponseDto<long>> CreateARRInvoiceAdjusmentRequest([FromBody] IEnumerable<APAROffsetCreateDto> data)
		{
			var response = new ResponseDto<long>();
			try
			{
				var accountInfo = User.GetAccountBasicInfo();

				var requestCreation = new RequestCreationDto<IEnumerable<APAROffsetCreateDto>>(data, User.GetAccountBasicInfo());

				long requestId = await _aparRepo.Create(requestCreation, accountInfo.Id);

				response.Result = requestId;
				response.Message = "Request Created Successfully";
				return response;
			}
			catch (Exception ex)
			{
				return response.Failed(ex.Message);
			}
		}

        [HttpPost("invoice/ari"), Authorize(Roles = "Requestor")]
        public async Task<ResponseDto<long>> CreateARIInvoiceAdjustmentRequest([FromBody] IEnumerable<ARInvoiceOffsettingCreateDto> data)
        {
            var response = new ResponseDto<long>();
            try
            {
                var accountInfo = User.GetAccountBasicInfo();

				var requestCreation = new RequestCreationDto<IEnumerable<ARInvoiceOffsettingCreateDto>>(data, User.GetAccountBasicInfo());

				long requestId = await _arRepo.Create(requestCreation, accountInfo.Id);

				response.Result = requestId;
				response.Message = "Request Created Successfully";
                return response;
            }
            catch (Exception ex)
            {
                return response.Failed(ex.Message);
            }
        }

        [HttpGet("approvals/receipt"), Authorize(Roles = "Approver")]
		public async Task<ResponseDto<IEnumerable<ReceiptAdjustmentRowDto>>> GetForApprovals(SearchRequestDto searchRequest)
		{
			var response = new ResponseDto<IEnumerable<ReceiptAdjustmentRowDto>>();
			try
			{
				response.Result = await _requestRepo.GetAllForApprovalsByType(searchRequest);
				return response;
			}
			catch (Exception ex)
			{
				return response.Failed(ex.Message);
			}
		}

		[HttpGet("submissions/receipt"), Authorize]
		public async Task<ResponseDto<IEnumerable<ReceiptAdjustmentRowDto>>> GetSubmissions(SearchRequestDto searchRequest)
		{
			var response = new ResponseDto<IEnumerable<ReceiptAdjustmentRowDto>>();
			try
			{
				var accountInfo = User.GetAccountBasicInfo();
				response.Result = await _requestRepo.GetSubmissions(searchRequest, accountInfo.Role, accountInfo.FullName);
				return response;
			}
			catch (Exception ex)
			{
				return response.Failed(ex.Message);
			}
		}
	}
}
