using ARAS.Blazor.Models.DTOs;
using ARAS.Blazor.Repositories.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ARAS.Blazor.Controllers
{
	[Route("api/ops")]
	[ApiController]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Ops")]
	public class OpsController : ControllerBase
	{
		private readonly IAccountRepository _accountService;
		private readonly ILogger<OpsController> _logger;

		public OpsController(IAccountRepository accountService, ILogger<OpsController> logger)
		{
			_accountService = accountService;
			_logger = logger;
		}

		[HttpGet("accounts")]
		public async Task<ResponseDto<IEnumerable<AccountRowDto>>> Get()
		{
			ResponseDto<IEnumerable<AccountRowDto>> _response = new();
			try
			{
				_response.Result = await _accountService.GetAll();
				return _response;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message);
				return _response.Failed(ex.Message);
			}
		}

		[HttpGet("roles")]
		public async Task<ResponseDto<IEnumerable<DropdownOptionDto>>> Roles()
		{
			ResponseDto<IEnumerable<DropdownOptionDto>> _response = new();
			try
			{
				_response.Result = await _accountService.GetAllRoleOptions();
				return _response;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message);
				return _response.Failed(ex.Message);
			}
		}

		[HttpGet("accounts/{id}")]
		public async Task<ResponseDto<AccountEditRequestDto>> Get(string id)
		{
			ResponseDto<AccountEditRequestDto> _response = new();
			try
			{
				_response.Result = await _accountService.GetById(id);
				return _response;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message);
				return _response.Failed(ex.Message);
			}
		}


		[HttpPost("accounts")]
		public async Task<ResponseDto> Post(AccountEditRequestDto request)
		{
			ResponseDto<string> _response = new();
			try
			{
				await _accountService.Update(request);
				_response.Result = "Success";
				_response.Message = "Account Successfully updated";
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
