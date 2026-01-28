using ARAS.Blazor.Models.DTOs;
using ARAS.Blazor.Repositories.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ARAS.Blazor.Controllers
{
	[Route("api/email")]
	[ApiController]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public class EmailController : ControllerBase
	{
		private readonly IEmailRepository _emailRepo;
		private readonly ILogger<EmailController> _logger;

		public EmailController(IEmailRepository emailRepo)
		{
			_emailRepo = emailRepo;
		}

		[HttpGet("all")]
		public async Task<ResponseDto<IEnumerable<string>>> GetAll()
		{
			ResponseDto<IEnumerable<string>> _response = new();
			try
			{
				//_response.Result = await _emailRepo.GetAll();
				_response.Result = [];
				return _response;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message);
				return _response.Failed(ex.Message);
			}
		}

		[HttpGet("approvers")]
		public async Task<ResponseDto<IEnumerable<string>>> GetApprovers()
		{
			ResponseDto<IEnumerable<string>> _response = new();
			try
			{
				//_response.Result = await _emailRepo.GetApprovers();
				_response.Result = [];
				return _response;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message);
				return _response.Failed(ex.Message);
			}
		}

		[HttpGet("validators")]
		public async Task<ResponseDto<IEnumerable<string>>> GetValidators()
		{
			ResponseDto<IEnumerable<string>> _response = new();
			try
			{
				//_response.Result = await _emailRepo.GetValidators();
				_response.Result = [];
				return _response;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message);
				return _response.Failed(ex.Message);
			}
		}

		[HttpGet("negate/{role}")]
		public async Task<ResponseDto<IEnumerable<string>>> GetForNegateEmails(string role)
		{
			ResponseDto<IEnumerable<string>> _response = new();
			try
			{
				//_response.Result = await _emailRepo.GetNegateRecipients(role);
				_response.Result = [];
				return _response;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message);
				return _response.Failed(ex.Message);
			}
		}

		[HttpGet("update/{role}")]
		public async Task<ResponseDto<IEnumerable<string>>> GetForUpdateEmails(string role)
		{
			ResponseDto<IEnumerable<string>> _response = new();
			try
			{
				//_response.Result = await _emailRepo.GetUpdateRecipients(role);
				_response.Result = [];
				return _response;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message);
				return _response.Failed(ex.Message);
			}
		}
	}
}
