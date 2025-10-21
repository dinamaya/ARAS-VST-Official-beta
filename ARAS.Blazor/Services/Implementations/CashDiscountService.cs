using ARAS.Blazor.App_Code.Globals;
using ARAS.Blazor.App_Code.Globals.Constants;
using ARAS.Blazor.App_Code.Globals.Enums;
using ARAS.Blazor.Models.DTOs;
using ARAS.Blazor.Services.Interfaces;
using System.Collections.Generic;
using ARAS.Blazor.App_Code.Globals.Extensions;
using Radzen;

namespace ARAS.Blazor.Services.Implementations
{
	public class CashDiscountService : ICashDiscountService
	{
		private readonly IBaseService _baseService;
		private readonly IConfigService _configService;
		private readonly IEmailService _emailService;
		private readonly DialogService _dialogService;

		public CashDiscountService(IBaseService baseService, IConfigService configService, DialogService dialogService, IEmailService emailService)
		{
			_baseService = baseService;
			_configService = configService;
			_dialogService = dialogService;
			_emailService = emailService;
		}

		public async Task Create(IEnumerable<CashDiscountRowDto> rows)
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

			var response = await _baseService.SendAsync<string>(new RequestDto<AdjustmentRequestCreationDto<CashDiscountCreateDto>>()
			{
				ApiType = ApiType.POST,
				URL = _configService.GetMainSSMSApiUrl("cash-discount/create"),
				Data = adjustmentRequestCreation
			});
		}

		public async Task Update(long requestId, IEnumerable<CashDiscountRowDto> rows)
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

			var response = await _baseService.SendAsync<string>(new RequestDto<AdjustmentRequestCreationDto<CashDiscountCreateDto>>()
			{
				ApiType = ApiType.POST,
				URL = _configService.GetMainSSMSApiUrl($"cash-discount/update/{requestId}"),
				Data = adjustmentRequestUpdate
			});

			Guards.ThrowInvalidOperationIf(!response.IsSuccess, "Failed to update cash-discount request");
		}

		public async Task Approve(long requestId)
		{
			var emails = await _emailService.GetValidators();
			var requestUpdateDto = new RequestUpdateDto(requestId, emails);

			var response = await _baseService.SendAsync<string>(new RequestDto<RequestUpdateDto>()
			{
				ApiType = ApiType.POST,
				URL = _configService.GetMainSSMSApiUrl($"approve/cdr"),
				Data = requestUpdateDto
			});

			Guards.ThrowInvalidOperationIf(!response.IsSuccess, "Failed to APPROVE the current request");
			Guards.ThrowInvalidOperationIf(!(response.IsSuccess && response.Result == "Success"), "Failed to APPROVE the current request");
		}

		public async Task Validate(long requestId)
		{
			var emails = await _emailService.GetAll();
			var requestUpdateDto = new RequestUpdateDto(requestId, emails);

			var response = await _baseService.SendAsync<string>(new RequestDto<RequestUpdateDto>()
			{
				ApiType = ApiType.POST,
				URL = _configService.GetMainSSMSApiUrl($"validate/cdr"),
				Data = requestUpdateDto
			});

			Guards.ThrowInvalidOperationIf(!response.IsSuccess, "Failed to VALIDATE the current request");
			Guards.ThrowInvalidOperationIf(!(response.IsSuccess && response.Result == "Success"), "Failed to VALIDATE the current request");
		}

		public async Task Decline(CreateDeclineDto createDecline)
		{
			var response = await _baseService.SendAsync<string>(new RequestDto<CreateDeclineDto>()
			{
				ApiType = ApiType.POST,
				URL = _configService.GetMainSSMSApiUrl($"decline/cdr"),
				Data = createDecline
			});

			Guards.ThrowInvalidOperationIf(!response.IsSuccess, "Failed to DECLINE the current request");
			Guards.ThrowInvalidOperationIf(!(response.IsSuccess && response.Result == "Success"), "Failed to DECLINE the current request");
		}

		public async Task Reject(long requestId)
		{
			var response = await _baseService.SendAsync<string>(new RequestDto()
			{
				ApiType = ApiType.POST,
				URL = _configService.GetMainSSMSApiUrl($"reject/cdr/{requestId}"),
			});

			Guards.ThrowInvalidOperationIf(!response.IsSuccess, "Failed to REJECT the current request");
			Guards.ThrowInvalidOperationIf(!(response.IsSuccess && response.Result == "Success"), "Failed to REJECT the current request");
		}

		public async Task<IEnumerable<TransactionRequestRowDto>> GetSubmissions()
		{
			var response = await _baseService.SendAsync<IEnumerable<TransactionRequestRowDto>>(new RequestDto()
			{
				URL = _configService.GetMainSSMSApiUrl("cash-discount/submissions"),
			});

			Guards.ThrowInvalidOperationIf(!response.IsSuccess, "Failed to fetch the submitted requests");

			return response.Result;
		}

		public async Task<IEnumerable<TransactionRequestRowDto>> GetApprovals()
		{
			var response = await _baseService.SendAsync<IEnumerable<TransactionRequestRowDto>>(new RequestDto()
			{
				URL = _configService.GetMainSSMSApiUrl("cash-discount/approvals"),
			});

			Guards.ThrowInvalidOperationIf(!response.IsSuccess, "Failed to fetch for approval requests");

			return response.Result;
		}

		public async Task<IEnumerable<TransactionRequestRowDto>> GetValidations()
		{
			var response = await _baseService.SendAsync<IEnumerable<TransactionRequestRowDto>>(new RequestDto()
			{
				URL = _configService.GetMainSSMSApiUrl("cash-discount/validations"),
			});

			Guards.ThrowInvalidOperationIf(!response.IsSuccess, "Failed to fetch for validation requests");

			return response.Result;
		}

		public async Task<IEnumerable<CashDiscountRowDto>> GetAdjustments(long requestId)
		{
			var response = await _baseService.SendAsync<IEnumerable<CashDiscountRowDto>>(new RequestDto()
			{
				URL = _configService.GetMainSSMSApiUrl($"adjustments/cdr/{requestId}"),
			});

			Guards.ThrowInvalidOperationIf(!response.IsSuccess, "Failed to fetch the adjustments");

			return response.Result;
		}

		public async Task<TransactionRequestRowDto> GetRequestDetails(long requestId)
		{
			var response = await _baseService.SendAsync<TransactionRequestRowDto>(new RequestDto()
			{
				URL = _configService.GetMainSSMSApiUrl($"requests/cdr/{requestId}"),
			});

			Guards.ThrowInvalidOperationIf(!response.IsSuccess, "Failed to fetch the request");

			return response.Result;
		}

		public async Task<bool> IsValid(CashDiscountCreateValidationDto cashDiscountCreateValidation)
		{
			var response = await _baseService.SendAsync<bool>(new RequestDto<CashDiscountCreateValidationDto>()
			{
				ApiType = ApiType.POST,
				Data = cashDiscountCreateValidation,
				URL = _configService.GetMainSSMSApiUrl($"adjustments/cdr/validate"),
			});

			return response.IsSuccess && !response.Result;
		}

	}
}
