using ARAS.Blazor.App_Code.Globals;
using ARAS.Blazor.App_Code.Globals.Enums;
using ARAS.Blazor.Models.DTOs;
using ARAS.Blazor.Services.Interfaces;

namespace ARAS.Blazor.Services.Implementations
{
	public class RequestService : IRequestService
	{
		private readonly IBaseService _baseService;
		private readonly IConfigService _configService;

		public RequestService(IBaseService baseService, IConfigService configService)
		{
			_baseService = baseService;
			_configService = configService;
		}

		public async Task<bool> IsApprovable(long requestId)
		{
			var response = await _baseService.SendAsync<bool>(new RequestDto()
			{
				URL = _configService.GetMainSSMSApiUrl($"requests/cdr/is-approvable/{requestId}"),
			});

			return response.IsSuccess && response.Result;
		}

		public async Task<bool> IsValidatable(long requestId)
		{
			var response = await _baseService.SendAsync<bool>(new RequestDto()
			{
				URL = _configService.GetMainSSMSApiUrl($"requests/cdr/is-validatable/{requestId}"),
			});

			return response.IsSuccess && response.Result;
		}
	}
}
