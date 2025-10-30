using ARAS.Blazor.App_Code.Globals;
using ARAS.Blazor.App_Code.Globals.Enums;
using ARAS.Blazor.Models.DTOs;
using ARAS.Blazor.Services.Interfaces;

namespace ARAS.Blazor.Services.Implementations
{
	public class OpsService : IOpsService
	{
		private readonly IBaseService _baseService;
		private readonly IConfigService _configService;

		public OpsService(IBaseService baseService, IConfigService configService)
		{
			_baseService = baseService;
			_configService = configService;
		}

		public async Task<IEnumerable<AccountRowDto>> GetAccounts()
		{
			var response = await _baseService.SendAsync< IEnumerable<AccountRowDto>>(new RequestDto<IEnumerable<AccountRowDto>>()
			{
				URL = _configService.GetOpsUrl("accounts")
			});

			return response.Result;
		}


		public async Task<AccountEditRequestDto> GetAccountById(string id)
		{
			var response = await _baseService.SendAsync<AccountEditRequestDto>(new RequestDto()
			{
				URL = _configService.GetOpsUrl($"accounts/{id}")
			});

			return response.Result;
		}

		public async Task<IEnumerable<DropdownOptionDto>> GetRoles()
		{
			var response = await _baseService.SendAsync<IEnumerable<DropdownOptionDto>>(new RequestDto()
			{
				URL = _configService.GetOpsUrl("roles")
			});

			return response.Result;
		}

		public async Task<bool> Update(AccountEditRequestDto data)
		{
			var response = await _baseService.SendAsync<string>(new RequestDto<AccountEditRequestDto>()
			{
				ApiType = ApiType.POST,
				Data = data,
				URL = _configService.GetOpsUrl("accounts")
			});

			return response.IsSuccess && response.Result.Equals("Success");
		}
	}
}
