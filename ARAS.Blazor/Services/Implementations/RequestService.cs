using ARAS.Blazor.App_Code.Globals;
using ARAS.Blazor.App_Code.Globals.Enums;
using ARAS.Blazor.Models.DTOs;
using ARAS.Blazor.Services.Interfaces;
using Azure.Core;

namespace ARAS.Blazor.Services.Implementations
{
	public class RequestService : IRequestService
	{
		private readonly IBaseService _baseService;
		private readonly IConfigService _configService;
		private readonly INoteService _noteService;

        public RequestService(IBaseService baseService, IConfigService configService, INoteService noteService)
        {
            _baseService = baseService;
            _configService = configService;
            _noteService = noteService;
        }

        public async Task<bool> IsUpdatable(long requestId) => await IsOnStatus(requestId, "is-updatable");
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

		public async Task<IEnumerable<InvoiceAdjustmentRowDto>> GetInvoiceAdjustmentRequests(SearchRequestDto data)
		{
			var response = await _baseService.SendAsync<IEnumerable<InvoiceAdjustmentRowDto>>(new RequestDto<SearchRequestDto>()
			{
				URL = _configService.GetRequestsUrl("approvals/invoice"),
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

		public async Task<IEnumerable<ReceiptAdjustmentRowDto>> GetReceiptAdjustmentSubmissions(SearchRequestDto data)
        {
			var response = await _baseService.SendAsync<IEnumerable<ReceiptAdjustmentRowDto>>(new RequestDto<SearchRequestDto>()
				{
					URL = _configService.GetRequestsUrl("submissions/receipt"),
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

		public async Task<ReceiptAdjustmentUpdateResponseDto> GetReceiptDetails(long requestId)
		{
			var response = await _baseService.SendAsync<ReceiptAdjustmentUpdateResponseDto>(new RequestDto()
				{
					URL = _configService.GetRequestsUrl($"receipt/{requestId}"),
				},
				onSuccessSendCallBack: async (resp) =>
				{
					await Task.Run(() =>
					{
						Guards.ThrowInvalidOperationIf(!resp.IsSuccess, "Failed to fetch the request details");
					});
				});

			return response.Result;
		}

		public async Task<IEnumerable<InvoiceAdjustmentRowDto>> GetInvoiceAdjustmentSubmissions(SearchRequestDto data)
		{
			var response = await _baseService.SendAsync<IEnumerable<InvoiceAdjustmentRowDto>>(new RequestDto<SearchRequestDto>()
				{
					URL = _configService.GetRequestsUrl("submissions/invoice"),
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

		public async Task Create(IEnumerable<BaseReceiptAdjustmentCreateDto> row, IEnumerable<NoteRowDto> notes)
		{

			var createResult = await _baseService.SendAsync<IEnumerable<ReceiptAdjustmentCreateResponseDto>>(new RequestDto<IEnumerable<BaseReceiptAdjustmentCreateDto>>()
				{
					ApiType = ApiType.POST,
					URL = _configService.GetRequestsUrl("receipt"),
					Data = row
				},
				onSuccessSendCallBack: (resp) =>
				{
					Guards.ThrowInvalidOperationIf(!resp.IsSuccess, "Failed to create request" + resp.Message);
					return Task.CompletedTask;
				}
			);

			foreach(var note in notes)
				note.Id = createResult.Result.Where(r => r.AdjustmentTypeCode == note.AdjustmentType).FirstOrDefault()?.RequestId.ToString() ?? string.Empty;

			await _noteService.Create(notes);
		}

		public async Task Update(long requestId, ReceiptAdjustmentUpdateRequestDto row, IEnumerable<NoteRowDto> notes)
		{
			if(row.AdjustmentAmount != 0 || !string.IsNullOrEmpty(row.Remarks) )
			{
				var createResult = await _baseService.SendAsync<long>(new RequestDto<ReceiptAdjustmentUpdateRequestDto>()
				{
					ApiType = ApiType.PUT,
					URL = _configService.GetRequestsUrl($"receipt/{requestId}"),
					Data = row
				},
					onSuccessSendCallBack: (resp) =>
					{
						Guards.ThrowInvalidOperationIf(!resp.IsSuccess, "Failed to update request" + resp.Message);
						return Task.CompletedTask;
					}
				);
			}

			await _noteService.Create(requestId, notes);
		}

		public async Task ApproveReceiptAdjustmentRequests(IEnumerable<long> data) => await UpdateAdjustmentStatus(data, _configService.GetApprovalsUrl());
		public async Task RejectReceiptAdjustmentRequests(IEnumerable<long> data) => await UpdateAdjustmentStatus(data, _configService.GetRejectionsUrl());
		public async Task DeclineReceiptAdjustmentRequests(IEnumerable<long> data) => await UpdateAdjustmentStatus(data, _configService.GetDeclinesUrl());

		private async Task UpdateAdjustmentStatus(IEnumerable<long> data, string route)
		{
			var response = await _baseService.SendAsync<string>(new RequestDto<IEnumerable<long>>()
			{
				URL = route,
				ApiType = ApiType.POST,
				Data = data
			},
				onSuccessSendCallBack: async (resp) =>
				{
					await Task.Run(() =>
					{
						Guards.ThrowInvalidOperationIf(!resp.IsSuccess, "Failed to update the request");
					});
				});
		}

    }
}
