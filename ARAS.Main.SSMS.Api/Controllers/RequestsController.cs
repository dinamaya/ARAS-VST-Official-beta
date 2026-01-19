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
		private readonly ICashDiscountRepository _cashDiscountRepo;
		private readonly IRequestRepository _requestRepo;

		public RequestsController(ILogger<RequestsController> logger, ICashDiscountRepository cashDiscountRepo, IRequestRepository requestRepo)
		{
			_logger = logger;
			_cashDiscountRepo = cashDiscountRepo;
			_requestRepo = requestRepo;
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

	}
}
