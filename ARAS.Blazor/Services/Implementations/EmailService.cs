using ARAS.Blazor.App_Code.Globals;
using ARAS.Blazor.Models.DTOs;
using ARAS.Blazor.Services.Interfaces;

namespace ARAS.Blazor.Services.Implementations
{
	public class EmailService : IEmailService
	{
		private readonly IBaseService _baseService;
		private readonly IConfigService _configService;

		public EmailService(IBaseService baseService, IConfigService configService)
		{
			_baseService = baseService;
			_configService = configService;
		}

		public async Task<IEnumerable<string>> GetAll()
		{
			var response = await _baseService.SendAsync<IEnumerable<string>>(new RequestDto()
			{
				URL = _configService.GetAuthEmailUrl("all"),
			});

			return response.Result;
		}

		public async Task<IEnumerable<string>> GetApprovers()
		{
			var response = await _baseService.SendAsync<IEnumerable<string>>(new RequestDto()
			{
				URL = _configService.GetAuthEmailUrl("approvers"),
			});

			Guards.ThrowNullReferenceIf(response.Result, "No approver emails found");

			return response.Result;
		}

		public async Task<IEnumerable<string>> GetValidators()
		{
			var response = await _baseService.SendAsync<IEnumerable<string>>(new RequestDto()
			{
				URL = _configService.GetAuthEmailUrl("validators"),
			});

			return response.Result;
		}
	}
}
