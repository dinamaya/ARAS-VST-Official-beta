using ARAS.Blazor.App_Code.Globals;
using ARAS.Blazor.App_Code.Globals.Enums;
using ARAS.Blazor.Models.DTOs;
using ARAS.Blazor.Services.Interfaces;
using Microsoft.AspNetCore.Routing;

namespace ARAS.Blazor.Services.Implementations
{
	public class BaseAdjustmentService<TCreate, TRow, TValidation> : IBaseAdjustmentService<TCreate, TRow, TValidation>
	{
		private readonly IBaseService _baseService;
		private readonly IConfigService _configService;
		private readonly IEmailService _emailService;
		private readonly INoteService _noteService;
		private readonly IAuthService _authService;

		public BaseAdjustmentService(IBaseService baseService, IConfigService configService, IEmailService emailService, INoteService noteService, IAuthService authService)
		{
			_baseService = baseService;
			_configService = configService;
			_emailService = emailService;
			_noteService = noteService;
			_authService = authService;
		}

		public async Task Create(List<TCreate> rows, IEnumerable<NoteRowDto> notes, string route)
		{
			var emails = await _emailService.GetApprovers();
			var adjustmentRequestCreation = new AdjustmentRequestCreationDto<TCreate>(rows, emails);

			var createResult = await _baseService.SendAsync<long>(new RequestDto<AdjustmentRequestCreationDto<TCreate>>()
				{
					ApiType = ApiType.POST,
					URL = route,
					Data = adjustmentRequestCreation
				},
				onSuccessSendCallBack: (resp) =>
				{
					Guards.ThrowInvalidOperationIf(!resp.IsSuccess, "Failed to create request" + resp.Message);
					return Task.CompletedTask;
				});

			await _noteService.Create(createResult.Result, notes);
		}

		public async Task Update(long requestId, IEnumerable<TCreate> rows, IEnumerable<NoteRowDto> notes, string route)
		{
			string role = await _authService.GetRole();
			var emails = await _emailService.GetUpdateRecipients(role);
			var adjustmentRequestUpdate = new AdjustmentRequestCreationDto<TCreate>(rows, emails);

			await _baseService.SendAsync<string>(
				new RequestDto<AdjustmentRequestCreationDto<TCreate>>()
				{
					ApiType = ApiType.POST,
					URL = $"{route}update/{requestId}",
					Data = adjustmentRequestUpdate
				},
				onSuccessSendCallBack: (resp) =>
				{
					Guards.ThrowInvalidOperationIf(!resp.IsSuccess, "Failed to update request" + resp.Message);
					return Task.CompletedTask;
				});

			await _noteService.Create(requestId, notes);
		}

		public async Task Approve(long requestId, IEnumerable<NoteRowDto> notes, string adjustmentTypeCode)
		{
			var emails = await _emailService.GetValidators();
			var requestUpdateDto = new RequestUpdateDto(requestId, emails);

			await _baseService.SendAsync<string>(
				new RequestDto<RequestUpdateDto>()
				{
					ApiType = ApiType.POST,
					URL = _configService.GetApprovalsUrl(adjustmentTypeCode),
					Data = requestUpdateDto
				},
				onSuccessSendCallBack: (resp) =>
				{
					Guards.ThrowInvalidOperationIf(!resp.IsSuccess, "Failed to APPROVE the current request" + resp.Message);
					Guards.ThrowInvalidOperationIf(!(resp.IsSuccess && resp.Result == "Success"), "Failed to APPROVE the current request" + resp.Message);
					return Task.CompletedTask;
				}
			);

			await _noteService.Create(requestId, notes);
		}

		public async Task Validate(long requestId, IEnumerable<NoteRowDto> notes, string adjustmentTypeCode)
		{
			var emails = await _emailService.GetAll();
			var requestUpdateDto = new RequestUpdateDto(requestId, emails);

			await _baseService.SendAsync<string>(
				new RequestDto<RequestUpdateDto>()
				{
					ApiType = ApiType.POST,
					URL = _configService.GetValidationsUrl(adjustmentTypeCode),
					Data = requestUpdateDto
				},
				onSuccessSendCallBack: (resp) =>
				{
					Guards.ThrowInvalidOperationIf(!resp.IsSuccess, "Failed to VALIDATE the current request" + resp.Message);
					Guards.ThrowInvalidOperationIf(!(resp.IsSuccess && resp.Result == "Success"), "Failed to VALIDATE the current request");
					return Task.CompletedTask;
				}
			);

			await _noteService.Create(requestId, notes);
		}

		public async Task Decline(NegateRequestDto createDecline, IEnumerable<NoteRowDto> notes, string adjustmentTypeCode)
		{
			string role = await _authService.GetRole();
			createDecline.ToEmail = await _emailService.GetNegateRecipients(role);

			await _baseService.SendAsync<string>(
				new RequestDto<NegateRequestDto>()
				{
					ApiType = ApiType.POST,
					URL = _configService.GetDeclinesUrl(adjustmentTypeCode),
					Data = createDecline
				},
				onSuccessSendCallBack: (resp) =>
				{
					Guards.ThrowInvalidOperationIf(!resp.IsSuccess, "Failed to DECLINE the current request" + resp.Message);
					Guards.ThrowInvalidOperationIf(!(resp.IsSuccess && resp.Result == "Success"), "Failed to DECLINE the current request");
					return Task.CompletedTask;
				}
			);

			await _noteService.Create(createDecline.RequestId, notes);
		}

		public async Task Reject(NegateRequestDto createReject, IEnumerable<NoteRowDto> notes, string adjustmentTypeCode)
		{
			string role = await _authService.GetRole();
			createReject.ToEmail = await _emailService.GetNegateRecipients(role);

			await _baseService.SendAsync<string>(
				new RequestDto<NegateRequestDto>()
				{
					ApiType = ApiType.POST,
					URL = _configService.GetRejectionsUrl(adjustmentTypeCode),
					Data = createReject
				},
				onSuccessSendCallBack: (resp) =>
				{
					Guards.ThrowInvalidOperationIf(!resp.IsSuccess, "Failed to REJECT the current request");
					Guards.ThrowInvalidOperationIf(!(resp.IsSuccess && resp.Result == "Success"), "Failed to REJECT the current request");
					return Task.CompletedTask;

				});

			await _noteService.Create(createReject.RequestId, notes);
		}

		public async Task<IEnumerable<TransactionRequestRowDto>> GetSubmissions(string adjustmenTypeCode) => await GetRows(adjustmenTypeCode, "submissions", "Failed to fetch the submitted requests");

		public async Task<IEnumerable<TransactionRequestRowDto>> GetApprovals(string adjustmenTypeCode) => await GetRows(adjustmenTypeCode, "approvals", "Failed to fetch the approved requests");

		public async Task<IEnumerable<TransactionRequestRowDto>> GetValidations(string adjustmenTypeCode) => await GetRows(adjustmenTypeCode, "validations", "Failed to fetch the validated requests");

		private async Task<IEnumerable<TransactionRequestRowDto>> GetRows(string adjustmenTypeCode, string adjustmentStage, string errorMessage)
		{
			var response = await _baseService.SendAsync<IEnumerable<TransactionRequestRowDto>>(
				new RequestDto()
				{
					URL = _configService.GetRequestsUrl($"{adjustmentStage}/{adjustmenTypeCode}"),
				},
				onSuccessSendCallBack: (resp) =>
				{
					Guards.ThrowInvalidOperationIf(!resp.IsSuccess, errorMessage);
					return Task.CompletedTask;
				});

			return response.Result;
		}
		
		public async Task<IEnumerable<TRow>> GetAdjustmentsByRequestIdAndTypeCode(long requestId, string adjustmentTypeCode)
		{
			var response = await _baseService.SendAsync<IEnumerable<TRow>>(new RequestDto()
			{
				URL = _configService.GetAdjustmentsUrl($"{adjustmentTypeCode}/{requestId}"),
			},
			onSuccessSendCallBack: (resp) =>
			{
				Guards.ThrowInvalidOperationIf(!resp.IsSuccess, "Failed to fetch the adjustments");
				return Task.CompletedTask;
			});

			return response.Result;
		}

		public async Task<bool> IsValid(string adjustmentTypeCode, TValidation inputValues)
		{
			var response = await _baseService.SendAsync<bool>(new RequestDto<TValidation>()
			{
				ApiType = ApiType.POST,
				URL = _configService.GetValidationsUrl($"input/{adjustmentTypeCode}"),
				Data = inputValues
			});

			Guards.ThrowInvalidOperationIf(!response.IsSuccess, "Failed to connect. Please Check internet connection or contact the administrator");
			Guards.ThrowInvalidOperationIf(!response.Result, "Inputs Invalid: " + response.Message);

			return response.Result;
		}
	}
}
