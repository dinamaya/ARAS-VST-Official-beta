using ARAS.Blazor.App_Code.Globals;
using ARAS.Blazor.App_Code.Globals.Enums;
using ARAS.Blazor.App_Code.Globals.Extensions;
using ARAS.Blazor.Models.DTOs;
using ARAS.Blazor.Services.Interfaces;
using ARAS.Main.Oracle.Api.Models.Dtos;
using Azure.Core;
using Microsoft.AspNetCore.Routing;

namespace ARAS.Blazor.Services.Implementations
{
	public class RequestService : IRequestService
	{
		private readonly IBaseService _baseService;
		private readonly IConfigService _configService;
		private readonly INoteService _noteService;
		private readonly IOracleStagingService _oracleStagingService;
		private readonly IAuthService _authService;

        public RequestService(IBaseService baseService, IConfigService configService, INoteService noteService, IOracleStagingService oracleStagingService, IAuthService authService)
        {
            _baseService = baseService;
            _configService = configService;
            _noteService = noteService;
            _oracleStagingService = oracleStagingService;
            _authService = authService;
        }

        public async Task<bool> IsUpdatable(long requestId) => await IsOnStatus(requestId, "is-updatable");
        public async Task<bool> IsApprovable(long requestId) => await IsOnStatus(requestId, "is-approvable");
		public async Task<bool> IsFinalApprover() => (await _authService.GetRole()).IsFsgApprover();
		public async Task<bool> IsValidatable(long requestId) => await IsOnStatus(requestId, "is-validatable");
		public async Task<bool> IsDeclined(long requestId) => await IsOnStatus(requestId, "is-declined");

        public async Task<int> GetPendingRequestCount()
        {
            var response = await _baseService.SendAsync<int>(new RequestDto()
            {
                URL = _configService.GetRequestsUrl("count/pending"),
            });

            return response.IsSuccess ? response.Result : 0;
        }

        public async Task<int> GetApprovedRequestCount()
        {
            var response = await _baseService.SendAsync<int>(new RequestDto()
            {
                URL = _configService.GetRequestsUrl("count/approved"),
            });

            return response.IsSuccess ? response.Result : 0;
        }

        public async Task<int> GetDeclinedRequestCount()
        {
            var response = await _baseService.SendAsync<int>(new RequestDto()
            {
                URL = _configService.GetRequestsUrl("count/declined"),
            });

            return response.IsSuccess ? response.Result : 0;
        }

        public async Task<int> GetResubmittedRequestCount()
        {
            var response = await _baseService.SendAsync<int>(new RequestDto()
            {
                URL = _configService.GetRequestsUrl("count/resubmitted"),
            });

            return response.IsSuccess ? response.Result : 0;
        }

        public async Task<int> GetPostedRequestCount()
        {
            var response = await _baseService.SendAsync<int>(new RequestDto()
            {
                URL = _configService.GetRequestsUrl("count/posted"),
            });

            return response.IsSuccess ? response.Result : 0;
        }

        public async Task<int> GetRejectedRequestCount()
        {
            var response = await _baseService.SendAsync<int>(new RequestDto()
            {
                URL = _configService.GetRequestsUrl("count/rejected"),
            });

            return response.IsSuccess ? response.Result : 0;
        }

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
			});

			Guards.ThrowInvalidOperationIf(!createResult.IsSuccess, createResult.Message);

			foreach (var note in notes)
				note.Id = createResult.Result.Where(r => r.AdjustmentTypeCode == note.AdjustmentType).FirstOrDefault()?.RequestId.ToString() ?? string.Empty;

			await _noteService.Create(notes);
		}

		public async Task Update(bool isUpdatable, long requestId, ReceiptAdjustmentUpdateRequestDto row, IEnumerable<NoteRowDto> notes)
		{
			bool isInputValid = row.AdjustmentAmount != 0 || !string.IsNullOrEmpty(row.Remarks);
			if (isUpdatable && isInputValid)
			{
				var createResult = await _baseService.SendAsync<long>(new RequestDto<ReceiptAdjustmentUpdateRequestDto>()
				{
					ApiType = ApiType.PUT,
					URL = _configService.GetRequestsUrl($"receipt/{requestId}"),
					Data = row
				});
				Guards.ThrowInvalidOperationIf(!createResult.IsSuccess, "Failed to update request" + createResult.Message);
			}

			await _noteService.Create(requestId, notes);
		}

		public async Task ApproveReceiptAdjustmentRequests(IEnumerable<long> data)
		{
			await UpdateAdjustmentStatus(data, _configService.GetApprovalsUrl());
			if (await IsFinalApprover())
			{
				var postingData = await GetReceiptStagingDataByRequestId(data);
				await _oracleStagingService.Create(postingData);
			}
		}

		public async Task ApproveInvoiceAdjustmentRequests(IEnumerable<long> data)
		{
			await UpdateAdjustmentStatus(data, _configService.GetApprovalsUrl());
		}

		public async Task RejectReceiptAdjustmentRequests(IEnumerable<long> data) => await UpdateAdjustmentStatus(data, _configService.GetRejectionsUrl());
		public async Task DeclineReceiptAdjustmentRequests(IEnumerable<long> data) => await UpdateAdjustmentStatus(data, _configService.GetDeclinesUrl());

		public async Task Approve(long requestId, IEnumerable<NoteRowDto> notes, IEnumerable<AdjustmentPostingDto> postingData)
		{
			await UpdateOneRequestStatus(requestId, _configService.GetApprovalsUrl(), "Approve", notes);
			if (postingData != null && await IsFinalApprover())
				await _oracleStagingService.Create(postingData);
		}

		public async Task Decline(long requestId, IEnumerable<NoteRowDto> notes) => await UpdateOneRequestStatus(requestId, _configService.GetDeclinesUrl(), "Decline", notes);
		public async Task Reject(long requestId, IEnumerable<NoteRowDto> notes) => await UpdateOneRequestStatus(requestId, _configService.GetRejectionsUrl(), "Approve", notes);

		private async Task UpdateAdjustmentStatus(IEnumerable<long> data, string route)
		{
			var response = await _baseService.SendAsync<string>(new RequestDto<IEnumerable<long>>()
			{
				URL = route,
				ApiType = ApiType.POST,
				Data = data
			});

			Guards.ThrowInvalidOperationIf(!response.IsSuccess, "Failed to update the request");
		}

		private async Task UpdateOneRequestStatus(long requestId, string route, string statusName, IEnumerable<NoteRowDto> notes)
		{
			var response = await _baseService.SendAsync<string>(new RequestDto<long>()
			{
				URL = route + requestId,
				ApiType = ApiType.POST,
			});

			Guards.ThrowInvalidOperationIf(!response.IsSuccess, response.Message);

			await _noteService.Create(requestId, notes);
		}

		public async Task<IEnumerable<AdjustmentPostingDto>> GetReceiptStagingDataByRequestId(long requestId)
        {
			var response = await _baseService.SendAsync<IEnumerable<AdjustmentPostingDto>>(new RequestDto<long>()
			{
				URL = _configService.GetAdjustmentsUrl($"stage/receipt/{requestId}"),
			});

			Guards.ThrowInvalidOperationIf(!response.IsSuccess, response.Message);

			return response.Result;
		}

        public async Task<IEnumerable<AdjustmentPostingDto>> GetReceiptStagingDataByRequestId(IEnumerable<long> requestIds)
        {
			var response = await _baseService.SendAsync<IEnumerable<AdjustmentPostingDto>>(new RequestDto<IEnumerable<long>>()
			{
				URL = _configService.GetAdjustmentsUrl($"stage/receipt"),
				Data = requestIds,
				ApiType = ApiType.POST
			});

			Guards.ThrowInvalidOperationIf(!response.IsSuccess, response.Message);

			return response.Result;
		}
    }
}