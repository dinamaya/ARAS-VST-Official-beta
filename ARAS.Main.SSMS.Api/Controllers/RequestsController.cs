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

		[HttpGet("is-validatable/{requestId:long}"), Authorize(Roles = "Validator")]
		public async Task<ResponseDto<bool>> IsValidatable(long requestId)
		{
			var response = new ResponseDto<bool>();
			try
			{
				response.Result = await _requestRepo.IsValidatable(requestId);
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

		[HttpGet("submissions/{adjustmentTypeCode}"), Authorize]
		public async Task<ResponseDto<IEnumerable<TransactionRequestRowDto>>> GetAllSubmissions(string adjustmentTypeCode)
		{
			var response = new ResponseDto<IEnumerable<TransactionRequestRowDto>>();
			try
			{
				response.Message = "";
				response.Result = await _requestRepo.GetAllSubmissionsByType(adjustmentTypeCode);
				return response;
			}
			catch (Exception ex)
			{
				return response.Failed(ex.Message);
			}
		}

		[HttpGet("approvals/{adjustmentTypeCode}"), Authorize(Roles = "Approver")]
		public async Task<ResponseDto<IEnumerable<TransactionRequestRowDto>>> GetForApprovals(string adjustmentTypeCode)
		{
			var response = new ResponseDto<IEnumerable<TransactionRequestRowDto>>();
			try
			{
				response.Result = await _requestRepo.GetAllForApprovalsByType(adjustmentTypeCode);
				return response;
			}
			catch (Exception ex)
			{
				return response.Failed(ex.Message);
			}
		}

		[HttpGet("validations/{adjustmentTypeCode}"), Authorize(Roles = "Validator")]
		public async Task<ResponseDto<IEnumerable<TransactionRequestRowDto>>> GetForValidations(string adjustmentTypeCode)
		{
			var response = new ResponseDto<IEnumerable<TransactionRequestRowDto>>();
			try
			{
				response.Result = await _requestRepo.GetAllForValidationsByType(adjustmentTypeCode);
				return response;
			}
			catch (Exception ex)
			{
				return response.Failed(ex.Message);
			}
		}
	}
}
