using ARAS.Blazor.App_Code.Globals;
using ARAS.Blazor.App_Code.Globals.Enums;
using ARAS.Blazor.Models.DTOs;
using ARAS.Blazor.Services.Interfaces;
using Radzen;

namespace ARAS.Blazor.Services.Implementations
{
    public class APAROffsetService : IAPAROffsetService
	{
		private readonly IBaseService _baseService;
		private readonly IConfigService _configService;
		private readonly INoteService _noteService;

        public APAROffsetService(
            IConfigService configService,
            IBaseService baseService,
            INoteService noteService)
        {
            _baseService = baseService;
            _configService = configService;
            _noteService = noteService;
        }

		public async Task Create(IEnumerable<APAROffsetAPRowDto> apRows, IEnumerable<APAROffsetARRowDto> arRows, IEnumerable<NoteRowDto> notes, APAROffsetARRowDto? writeOffRow = null)
		{
			var requestsDto = ToCreateDto(apRows, arRows, writeOffRow);

			var createResult = await _baseService.SendAsync<long>(new RequestDto<IEnumerable<APAROffsetCreateDto>>()
			{
				ApiType = ApiType.POST,
				URL = _configService.GetRequestsUrl("invoice/arr"),
				Data = requestsDto
			});

			Guards.ThrowInvalidOperationIf(!createResult.IsSuccess, "Failed to create request" + createResult.Message);

			await _noteService.Create(createResult.Result, notes);
		}

		public async Task Update(bool isUpdatable, long requestId, IEnumerable<APAROffsetAPRowDto> apRows, IEnumerable<APAROffsetARRowDto> arRows, IEnumerable<NoteRowDto> notes, APAROffsetARRowDto? writeOffRow = null)
        {
			if(isUpdatable)
			{
				var requestsDto = ToCreateDto(apRows, arRows, writeOffRow);

				var result = await _baseService.SendAsync<long>(new RequestDto<IEnumerable<APAROffsetCreateDto>>()
				{
					ApiType = ApiType.PUT,
					URL = _configService.GetRequestsUrl($"invoice/arr/{requestId}"),
					Data = requestsDto
				});

				Guards.ThrowInvalidOperationIf(!result.IsSuccess, "Failed to update request" + result.Message);
			}

			await _noteService.Create(requestId, notes);
		}

		public async Task<APAROffsetRowDto> GetAdjustments(long requestId)
		{
			var arResponse = await _baseService.SendAsync<APAROffsetRowDto>(new RequestDto()
				{
					URL = _configService.GetAdjustmentsUrl($"aar/{requestId}"),
				},
				onSuccessSendCallBack: async (resp) =>
				{
					await Task.Run(() =>
					{
						Guards.ThrowInvalidOperationIf(!resp.IsSuccess, "Failed to fetch the AR adjustments");
					});
				});

			return arResponse.Result;
		}

		private static List<APAROffsetCreateDto> ToCreateDto(IEnumerable<APAROffsetAPRowDto> apRows, IEnumerable<APAROffsetARRowDto> arRows, APAROffsetARRowDto? writeOffRow)
		{
			var arInvoiceRows = arRows
				// Write-off rows intentionally have no invoice number. Any row with an
				// invoice number is therefore an AR invoice row and must be submitted.
				.Where(r => !string.IsNullOrWhiteSpace(r.InvoiceNumber))
				.ToList();
			var arInvoiceContext = arInvoiceRows.FirstOrDefault();

			var apRequests = apRows.Select(r => new APAROffsetCreateDto()
			{
				InvoiceNumber = r.InvoiceNumber,
				Amount = r.InvoiceAmount,
				InvoiceAmount = r.InvoiceAmount,
				Type = "AP",
				InvoiceDate = r.InvoiceDate,
				CustomerName = r.CustomerName,
				CustomerNumber = r.CustomerNumber,
				CustomerTrxId = r.CustomerTrxId,
				PaymentScheduleId = r.PaymentScheduleId,
				CurrencyCode = r.CurrencyCode,
				GlAccountingDate = r.GlAccountingDate
			});

			var arRequests = arInvoiceRows
				.Select(r => new APAROffsetCreateDto()
			{
				InvoiceNumber = r.InvoiceNumber,
				Amount = r.Amount,
				InvoiceAmount = r.InvoiceAmount,
				Type = "AR",
				ReasonCode = null,
				InvoiceDate = DateTime.Now,
				CustomerName = r.CustomerName,
				CustomerNumber = r.CustomerNumber,
				CustomerTrxId = r.CustomerTrxId,
				PaymentScheduleId = r.PaymentScheduleId,
				CurrencyCode = r.CurrencyCode,
				GlAccountingDate = r.GlAccountingDate
			});

			var writeOffRequests = Enumerable.Empty<APAROffsetCreateDto>();
			if (writeOffRow is not null)
			{
				var contextRow = arInvoiceContext ?? writeOffRow;
				writeOffRequests =
				[
					new APAROffsetCreateDto
					{
						InvoiceNumber = string.Empty,
						Amount = writeOffRow.Amount,
						InvoiceAmount = 0,
						Type = "AR",
						ReasonCode = writeOffRow.AdjustmentReason,
						InvoiceDate = DateTime.Now,
						CustomerName = contextRow.CustomerName,
						CustomerNumber = contextRow.CustomerNumber,
						CustomerTrxId = writeOffRow.CustomerTrxId ?? contextRow.CustomerTrxId,
						PaymentScheduleId = writeOffRow.PaymentScheduleId ?? contextRow.PaymentScheduleId,
						CurrencyCode = writeOffRow.CurrencyCode ?? contextRow.CurrencyCode,
						GlAccountingDate = writeOffRow.GlAccountingDate ?? contextRow.GlAccountingDate
					}
				];
			}

			return apRequests.Concat(arRequests).Concat(writeOffRequests).ToList();
		}
	}
}
