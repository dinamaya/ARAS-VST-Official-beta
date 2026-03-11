using ARAS.Blazor.App_Code.Globals.Extensions;
using ARAS.Main.SSMS.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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

		[HttpGet("is-updatable/{requestId:long}")]
		public async Task<ResponseDto<bool>> IsUpdatable(long requestId)
		{
			var response = new ResponseDto<bool>();
			try
			{
				response.Result = await _requestRepo.IsUpdatable(requestId);
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message);
				return response.Failed(ex.Message);
			}
		}

		[HttpGet("is-approvable/{requestId:long}")]
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

        [HttpGet("count/pending"), Authorize]
        public async Task<ResponseDto<int>> GetPendingRequestCount()
        {
            var response = new ResponseDto<int>();
            try
            {
                var accountInfo = User.GetAccountBasicInfo();
                response.Result = await _requestRepo.GetPendingRequestCount(accountInfo.Id);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return response.Failed(ex.Message);
            }
        }

        [HttpGet("count/approved"), Authorize]
        public async Task<ResponseDto<int>> GetApprovedRequestCount()
        {
            var response = new ResponseDto<int>();
            try
            {
                var accountInfo = User.GetAccountBasicInfo();
                response.Result = await _requestRepo.GetApprovedRequestCount(accountInfo.Id);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return response.Failed(ex.Message);
            }
        }

        [HttpGet("count/declined"), Authorize]
        public async Task<ResponseDto<int>> GetDeclinedRequestCount()
        {
            var response = new ResponseDto<int>();
            try
            {
                var accountInfo = User.GetAccountBasicInfo();
                response.Result = await _requestRepo.GetDeclinedRequestCount(accountInfo.Id);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return response.Failed(ex.Message);
            }
        }

        [HttpGet("count/resubmitted"), Authorize]
        public async Task<ResponseDto<int>> GetResubmittedRequestCount()
        {
            var response = new ResponseDto<int>();
            try
            {
                var accountInfo = User.GetAccountBasicInfo();
                response.Result = await _requestRepo.GetResubmittedRequestCount(accountInfo.Id);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return response.Failed(ex.Message);
            }
        }

        [HttpGet("count/posted"), Authorize]
        public async Task<ResponseDto<int>> GetPostedRequestCount()
        {
            var response = new ResponseDto<int>();
            try
            {
                var accountInfo = User.GetAccountBasicInfo();
                response.Result = await _requestRepo.GetPostedRequestCount(accountInfo.Id);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return response.Failed(ex.Message);
            }
        }

        [HttpGet("count/rejected"), Authorize]
        public async Task<ResponseDto<int>> GetRejectedRequestCount()
        {
            var response = new ResponseDto<int>();
            try
            {
                var accountInfo = User.GetAccountBasicInfo();
                response.Result = await _requestRepo.GetRejectedRequestCount(accountInfo.Id);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return response.Failed(ex.Message);
            }
        }

        [HttpPost("receipt"), Authorize(Roles = "Requestor")]
		public async Task<ResponseDto<IEnumerable<ReceiptAdjustmentCreateResponseDto>>> CreateReceiptAdjusmentRequest([FromBody] IEnumerable<BaseReceiptAdjustmentCreateDto> data)
		{
			var response = new ResponseDto<IEnumerable<ReceiptAdjustmentCreateResponseDto>>();
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


		[HttpPut("receipt/{requestId:long}"), Authorize(Roles = "Requestor")]
		public async Task<ResponseDto<long>> UpdateReceiptAdjusmentRequest(long requestId, [FromBody] ReceiptAdjustmentUpdateRequestDto data)
		{
			var response = new ResponseDto<long>();
			try
			{
				var account = User.GetAccountBasicInfo();
				response.Result = await _receiptAdjustmentRepo.UpdateAsync(requestId, data, account.Id);
				return response;
			}
			catch (Exception ex)
			{
				return response.Failed(ex.Message);
			}
		}

		[HttpPut("invoice/arr/{requestId:long}"), Authorize(Roles = "Requestor")]
		public async Task<ResponseDto<long>> UpdateAPARRequest(long requestId, [FromBody] IEnumerable<APAROffsetCreateDto> data)
		{
			var response = new ResponseDto<long>();
			try
			{
				var requestCreation = new RequestCreationDto<IEnumerable<APAROffsetCreateDto>>(data, User.GetAccountBasicInfo());
				response.Result = await _aparRepo.Update(requestId, requestCreation, requestCreation.CreatorId);
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

		[HttpPut("invoice/ari/{requestId:long}"), Authorize(Roles = "Requestor")]
		public async Task<ResponseDto<long>> UpdateARIRequest(long requestId, [FromBody] IEnumerable<ARInvoiceOffsettingCreateDto> data)
		{
			var response = new ResponseDto<long>();
			try
			{
				var requestCreation = new RequestCreationDto<IEnumerable<ARInvoiceOffsettingCreateDto>>(data, User.GetAccountBasicInfo());
				response.Result = await _arRepo.Update(requestId, requestCreation, requestCreation.CreatorId);
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

		[HttpGet("approvals/receipt"), Authorize(Roles = "CNC Approver,FSG Validator,FSG Approver")]
		public async Task<ResponseDto<IEnumerable<ReceiptAdjustmentRowDto>>> GetReceiptForApprovals(SearchRequestDto searchRequest)
		{
			var response = new ResponseDto<IEnumerable<ReceiptAdjustmentRowDto>>();
			try
			{
				var accountInfo = User.GetAccountBasicInfo();
				response.Result = await _requestRepo.GetReceiptAdjustmentApprovals(searchRequest, accountInfo.Role);
				return response;
			}
			catch (Exception ex)
			{
				return response.Failed(ex.Message);
			}
		}

		[HttpGet("approvals/invoice"), Authorize(Roles = "CNC Approver,FSG Validator,FSG Approver")]
		public async Task<ResponseDto<IEnumerable<InvoiceAdjustmentRowDto>>> GetInvoiceForApprovals(SearchRequestDto searchRequest)
		{
			var response = new ResponseDto<IEnumerable<InvoiceAdjustmentRowDto>>();
			try
			{
				var accountInfo = User.GetAccountBasicInfo();
				response.Result = await _requestRepo.GetInvoicedjustmentApprovals(searchRequest, accountInfo.Role);
				return response;
			}
			catch (Exception ex)
			{
				return response.Failed(ex.Message);
			}
		}

		[HttpGet("submissions/receipt"), Authorize]
		public async Task<ResponseDto<IEnumerable<ReceiptAdjustmentRowDto>>> GetReceiptSubmissions(SearchRequestDto searchRequest)
		{
			var response = new ResponseDto<IEnumerable<ReceiptAdjustmentRowDto>>();
			try
			{
				var accountInfo = User.GetAccountBasicInfo();
				response.Result = await _requestRepo.GetReceiptAdjustmentSubmissions(searchRequest, accountInfo.Role, accountInfo.FullName);
				return response;
			}
			catch (Exception ex)
			{
				return response.Failed(ex.Message);
			}
		}

		[HttpGet("submissions/invoice"), Authorize]
		public async Task<ResponseDto<IEnumerable<InvoiceAdjustmentRowDto>>> GetInvoiceSubmissions(SearchRequestDto searchRequest)
		{
			var response = new ResponseDto<IEnumerable<InvoiceAdjustmentRowDto>>();
			try
			{
				var accountInfo = User.GetAccountBasicInfo();
				response.Result = await _requestRepo.GetInvoiceAdjustmentSubmissions(searchRequest, accountInfo.Role, accountInfo.FullName);
				return response;
			}
			catch (Exception ex)
			{
				return response.Failed(ex.Message);
			}
		}

		[HttpGet("receipt/{requestId:long}"), Authorize]
		public async Task<ResponseDto<ReceiptAdjustmentUpdateResponseDto>> GetReceiptAdjustmentById(long requestId)
		{
			var response = new ResponseDto<ReceiptAdjustmentUpdateResponseDto>();
			try
			{
				response.Result = await _receiptAdjustmentRepo.GetDetailsById(requestId);
				return response;
			}
			catch (Exception ex)
			{
				return response.Failed(ex.Message);
			}
		}
	}
}
