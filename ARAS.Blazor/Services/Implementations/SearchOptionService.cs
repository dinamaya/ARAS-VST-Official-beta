
using ARAS.Blazor.Models.DTOs;
using ARAS.Blazor.Services.Interfaces;

namespace ARAS.Blazor.Services.Implementations
{
	public class SearchOptionService : ISearchOptionService
	{
		private readonly IBaseService _baseService;
		private readonly IConfigService _configService;

		public SearchOptionService(IBaseService baseService, IConfigService configService)
		{
			_baseService = baseService;
			_configService = configService;
		}

		public async Task<IEnumerable<string>> GetReasonCodes()
		{
			var response = await _baseService.SendAsync<IEnumerable<string>>(new RequestDto()
			{
				URL = _configService.GetOracleAdjustmentApiUrl("reason-codes"),
			});

			return response.Result;
		}
	}
}
