using ARAS.Auth.Api.Models.Dtos;
using ARAS.Auth.Api.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ARAS.Auth.Api.Controllers
{
	[Route("api/email")]
	[ApiController]
	public class EmailController : ControllerBase
	{
		private readonly IEmailService _emailService;
		private readonly ILogger<EmailController> _logger;

		public EmailController(IEmailService emailService)
		{
			_emailService = emailService;
		}

		[HttpGet("all")]
		public async Task<ResponseDto<IEnumerable<string>>> GetAll()
		{
			ResponseDto<IEnumerable<string>> _response = new();
			try
			{
				_response.Result = await _emailService.GetAll();
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
				_response.Result = await _emailService.GetApprovers();
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
				_response.Result = await _emailService.GetValidators();
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
