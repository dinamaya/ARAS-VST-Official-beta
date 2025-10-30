using ARAS.Blazor.App_Code.Globals;
using ARAS.Blazor.App_Code.Globals.Enums;
using ARAS.Blazor.Models.DTOs;
using ARAS.Blazor.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Radzen;

namespace ARAS.Blazor.Services.Implementations
{
	public class CashDiscountService : ICashDiscountService
	{
		private readonly IBaseService _baseService;
		private readonly IConfigService _configService;
		private readonly IEmailService _emailService;
		private readonly INoteService _noteService;
		private readonly DialogService _dialogService;

		public CashDiscountService(IBaseService baseService, IConfigService configService, DialogService dialogService, IEmailService emailService, INoteService noteService)
		{
			_baseService = baseService;
			_configService = configService;
			_dialogService = dialogService;
			_emailService = emailService;
			_noteService = noteService;
		}

		public async Task Create(IEnumerable<CashDiscountRowDto> rows, IEnumerable<NoteRowDto> notes)
		{
			var requestsDto = rows.Select(r => new CashDiscountCreateDto()
			{
				DiscountValue = r.DiscountValue,
				InvoiceAmount = r.InvoiceAmount,
				InvoiceDate = DateTime.Parse(r.InvoiceDate),
				InvoiceNumber = r.InvoiceNumber,
				CustomerName = r.CustomerName,
				CustomerNumber = r.CustomerNumber,
				Remarks = r.Remarks,
			}).ToList();

			var emails = await _emailService.GetApprovers();
			var adjustmentRequestCreation = new AdjustmentRequestCreationDto<CashDiscountCreateDto>(requestsDto, emails);

			var createResult = await _baseService.SendAsync<long>(new RequestDto<AdjustmentRequestCreationDto<CashDiscountCreateDto>>()
			{
				ApiType = ApiType.POST,
				URL = _configService.GetCashDiscountsUrl("create"),
				Data = adjustmentRequestCreation
			});

			await _noteService.Create(createResult.Result, notes);
		}

		public async Task Update(long requestId, IEnumerable<CashDiscountRowDto> rows, IEnumerable<NoteRowDto> notes)
		{
			var requestsDto = rows.Select(r => new CashDiscountCreateDto()
			{
				DiscountValue = r.DiscountValue,
				InvoiceAmount = r.InvoiceAmount,
				InvoiceDate = DateTime.Parse(r.InvoiceDate),
				InvoiceNumber = r.InvoiceNumber,
				CustomerName = r.CustomerName,
				CustomerNumber = r.CustomerNumber,
				Remarks = r.Remarks,
			}).ToList();

			var emails = await _emailService.GetApprovers();
			var adjustmentRequestUpdate = new AdjustmentRequestCreationDto<CashDiscountCreateDto>(requestsDto, emails);

			await _baseService.SendAsync<string>(
				new RequestDto<AdjustmentRequestCreationDto<CashDiscountCreateDto>>()
				{
					ApiType = ApiType.POST,
					URL = _configService.GetCashDiscountsUrl($"update/{requestId}"),
					Data = adjustmentRequestUpdate
				},
				onSuccessSendCallBack: async (resp) =>
				{
					await Task.Run(() =>
					{
						Guards.ThrowInvalidOperationIf(!resp.IsSuccess, "Failed to update cash-discount request");
					});
				}
			);
		
			await _noteService.Create(requestId, notes);
		}

		public async Task Approve(long requestId, IEnumerable<NoteRowDto> notes)
		{
			var emails = await _emailService.GetValidators();
			var requestUpdateDto = new RequestUpdateDto(requestId, emails);

			await _baseService.SendAsync<string>(
				new RequestDto<RequestUpdateDto>()
				{
					ApiType = ApiType.POST,
					URL = _configService.GetApprovalsUrl("cdr"),
					Data = requestUpdateDto
				},
				onSuccessSendCallBack: async (resp) =>
				{
					await Task.Run(() =>
					{
						Guards.ThrowInvalidOperationIf(!resp.IsSuccess, "Failed to APPROVE the current request");
						Guards.ThrowInvalidOperationIf(!(resp.IsSuccess && resp.Result == "Success"), "Failed to APPROVE the current request");
					});
				}
			);

			await _noteService.Create(requestId, notes);
		}

		public async Task Validate(long requestId, IEnumerable<NoteRowDto> notes)
		{
			var emails = await _emailService.GetAll();
			var requestUpdateDto = new RequestUpdateDto(requestId, emails);

			await _baseService.SendAsync<string>(
				new RequestDto<RequestUpdateDto>()
				{
					ApiType = ApiType.POST,
					URL = _configService.GetValidationsUrl("cdr"),
					Data = requestUpdateDto
				},
				onSuccessSendCallBack: async (resp) =>
				{
					await Task.Run(() =>
					{
						Guards.ThrowInvalidOperationIf(!resp.IsSuccess, "Failed to VALIDATE the current request");
						Guards.ThrowInvalidOperationIf(!(resp.IsSuccess && resp.Result == "Success"), "Failed to VALIDATE the current request");
					});
				});

			await _noteService.Create(requestId, notes);
		}

		public async Task Decline(NegateRequestDto createDecline, IEnumerable<NoteRowDto> notes)
		{
			createDecline.ToEmail = await _emailService.GetAll();

			await _baseService.SendAsync<string>(
				new RequestDto<NegateRequestDto>()
				{
					ApiType = ApiType.POST,
					URL = _configService.GetDeclinesUrl("cdr"),
					Data = createDecline
				},
				onSuccessSendCallBack: async (resp) =>
				{
					await Task.Run(() =>
					{
						Guards.ThrowInvalidOperationIf(!resp.IsSuccess, "Failed to DECLINE the current request");
						Guards.ThrowInvalidOperationIf(!(resp.IsSuccess && resp.Result == "Success"), "Failed to DECLINE the current request");
					});
				});

			await _noteService.Create(createDecline.RequestId, notes);
		}

		public async Task Reject(NegateRequestDto createReject, IEnumerable<NoteRowDto> notes)
		{
			createReject.ToEmail = await _emailService.GetAll();

			await _baseService.SendAsync<string>(
				new RequestDto<NegateRequestDto>()
				{
					ApiType = ApiType.POST,
					URL = _configService.GetRejectionsUrl("cdr"),
					Data = createReject
				},
				onSuccessSendCallBack: async (resp) =>
				{
					await Task.Run(() =>
					{
						Guards.ThrowInvalidOperationIf(!resp.IsSuccess, "Failed to REJECT the current request");
						Guards.ThrowInvalidOperationIf(!(resp.IsSuccess && resp.Result == "Success"), "Failed to REJECT the current request");
					});
				});

			await _noteService.Create(createReject.RequestId, notes);
		}

		public async Task<IEnumerable<TransactionRequestRowDto>> GetSubmissions()
		{
			var response = await _baseService.SendAsync<IEnumerable<TransactionRequestRowDto>>(
				new RequestDto()
				{
					URL = _configService.GetCashDiscountsUrl("submissions"),
				},
				onSuccessSendCallBack: async (resp) =>
				{
					await Task.Run(() =>
					{
						Guards.ThrowInvalidOperationIf(!resp.IsSuccess, "Failed to fetch the submitted requests");
					});
				});

			return response.Result;
		}

		public async Task<IEnumerable<TransactionRequestRowDto>> GetApprovals()
		{
			var response = await _baseService.SendAsync<IEnumerable<TransactionRequestRowDto>>(new RequestDto()
				{
					URL = _configService.GetCashDiscountsUrl("approvals"),
				},
				onSuccessSendCallBack: async (resp) =>
				{
					await Task.Run(() =>
					{
						Guards.ThrowInvalidOperationIf(!resp.IsSuccess, "Failed to fetch for approval requests");
					});
				});

			return response.Result;
		}

		public async Task<IEnumerable<TransactionRequestRowDto>> GetValidations()
		{
			var response = await _baseService.SendAsync<IEnumerable<TransactionRequestRowDto>>(new RequestDto()
				{
					URL = _configService.GetCashDiscountsUrl("validations"),
				},
				onSuccessSendCallBack: async (resp) =>
				{
					await Task.Run(() =>
					{
						Guards.ThrowInvalidOperationIf(!resp.IsSuccess, "Failed to fetch for validation requests");
					});
				});

			return response.Result;
		}

		public async Task<IEnumerable<CashDiscountRowDto>> GetAdjustments(long requestId)
		{
			var response = await _baseService.SendAsync<IEnumerable<CashDiscountRowDto>>(new RequestDto()
				{
					URL = _configService.GetAdjustmentsUrl($"cdr/{requestId}"),
				},
				onSuccessSendCallBack: async (resp) =>
				{
					await Task.Run(() =>
					{
						Guards.ThrowInvalidOperationIf(!resp.IsSuccess, "Failed to fetch the adjustments");
					});
				});

			return response.Result;
		}

		public async Task<TransactionRequestRowDto> GetRequestDetails(long requestId)
		{
			var response = await _baseService.SendAsync<TransactionRequestRowDto>(new RequestDto()
				{
					URL = _configService.GetRequestsUrl($"cdr/{requestId}"),
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

		public async Task<bool> IsValid(CashDiscountCreateValidationDto cashDiscountCreateValidation)
		{
			var response = await _baseService.SendAsync<bool>(new RequestDto<CashDiscountCreateValidationDto>()
			{
				ApiType = ApiType.POST,
				Data = cashDiscountCreateValidation,
				URL = _configService.GetAdjustmentsUrl($"cdr/validate"),
			});

			return response.IsSuccess && !response.Result;
		}

	}
}
