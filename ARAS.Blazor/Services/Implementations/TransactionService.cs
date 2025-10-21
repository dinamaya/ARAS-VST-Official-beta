using ARAS.Blazor.App_Code.Globals;
using ARAS.Blazor.Models.DTOs;
using ARAS.Blazor.Services.Interfaces;

namespace ARAS.Blazor.Services.Implementations
{
	public class TransactionService : ITransactionService
	{
		private readonly IBaseService _baseService;
		private readonly IConfigService _configService;

		public TransactionService(IBaseService baseService, IConfigService configService)
		{
			_baseService = baseService;
			_configService = configService;
		}

		public async Task<IEnumerable<TransactionHistoryDto>> GetHistoryByRequestId(long requestId)
		{
			var result = await _baseService.SendAsync<IEnumerable<TransactionHistoryDto>>(new RequestDto()
			{
				URL = _configService.GetMainSSMSApiUrl($"transactions/history/{requestId}")
			});

			return result.Result ?? [];
		}
	}
}
