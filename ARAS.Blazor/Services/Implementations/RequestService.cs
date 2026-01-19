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

		public async Task<bool> IsApprovable(long requestId) => await IsOnStatus(requestId, "is-approvable");
		public async Task<bool> IsValidatable(long requestId) => await IsOnStatus(requestId, "is-validatable");
		public async Task<bool> IsDeclined(long requestId) => await IsOnStatus(requestId, "is-declined");

		public async Task<TransactionRequestRowDto> GetRequestDetails(long requestId)
		{
			var response = await _baseService.SendAsync<TransactionRequestRowDto>(new RequestDto()
				{
					URL = _configService.GetRequestsUrl($"details/{requestId}"),
				},
				onSuccessSendCallBack: async (resp) =>
				{
					await Task.Run(() =>
					{
						Guards.ThrowInvalidOperationIf(!resp.IsSuccess, "Failed to fetch the request");
					});
				});

			return response.Result;
		}

		private async Task<bool> IsOnStatus(long requestId, string stageStatus)
		{
			var response = await _baseService.SendAsync<bool>(new RequestDto()
			{
				URL = _configService.GetRequestsUrl($"{stageStatus}/{requestId}"),
			});

			return response.IsSuccess && response.Result;
		}

        public async Task<IEnumerable<ReceiptAdjustmentRowDto>> GetReceiptAdjustmentRequests(SearchRequestDto data)
        {
			var response = await _baseService.SendAsync<IEnumerable<ReceiptAdjustmentRowDto>>(new RequestDto<SearchRequestDto>()
				{
					URL = _configService.GetRequestsUrl("approvals/receipt"),
					Data = data
				},
				onSuccessSendCallBack: async (resp) =>
				{
					await Task.Run(() =>
					{
						Guards.ThrowInvalidOperationIf(!resp.IsSuccess, "Failed to fetch the request");
					});
				});

			return response.Result;
		}

        public async Task ApproveReceiptAdjustmentRequests(IEnumerable<long> data)
        {
			var response = await _baseService.SendAsync<string>(new RequestDto<IEnumerable<long>>()
				{
					URL = _configService.GetApprovalsUrl(),
					ApiType = ApiType.POST,
					Data = data
				},
				onSuccessSendCallBack: async (resp) =>
				{
					await Task.Run(() =>
					{
						Guards.ThrowInvalidOperationIf(!resp.IsSuccess, "Failed to fetch the request");
					});
				});
		}
    }
}
