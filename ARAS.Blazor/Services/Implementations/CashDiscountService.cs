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
		private readonly DialogService _dialogService;

		public CashDiscountService(IBaseService baseService, IConfigService configService, DialogService dialogService)
		{
			_baseService = baseService;
			_configService = configService;
			_dialogService = dialogService;
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

			var response = await _baseService.SendAsync<string>(new RequestDto()
			{
				ApiType = ApiType.POST,
				URL = _configService.GetMainSSMSApiUrl("cash-discount"),
				Data = requestsDto
			});

			Guards.ThrowInvalidOperationIf(!response.IsSuccess, "Failed to create cash-discount request");
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

		public async Task<bool> ApproveAdjustment(long requestId)
		{
			var response = await _baseService.SendAsync<string>(new RequestDto()
			{
				ApiType = ApiType.POST,
				URL = _configService.GetMainSSMSApiUrl($"approve/cdr/{requestId}"),
			});

			Guards.ThrowInvalidOperationIf(!response.IsSuccess, "Failed to update the status of the current request");

			return response.IsSuccess && response.Result == "Success";
		}

		public async Task<bool> ValidateAdjustment(long requestId)
		{
			var response = await _baseService.SendAsync<string>(new RequestDto()
			{
				ApiType = ApiType.POST,
				URL = _configService.GetMainSSMSApiUrl($"validate/cdr/{requestId}"),
			});

			Guards.ThrowInvalidOperationIf(!response.IsSuccess, "Failed to update the status of the current request");

			return response.IsSuccess && response.Result == "Success";
		}
	}
}
