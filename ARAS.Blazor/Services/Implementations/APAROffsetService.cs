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

        public async Task Create(IEnumerable<APAROffsetAPRowDto> apRows, IEnumerable<APAROffsetARRowDto> arRows, IEnumerable<NoteRowDto> notes)
		{
			var requestsDto = ToCreateDto(apRows, arRows);

			var createResult = await _baseService.SendAsync<long>(new RequestDto<IEnumerable<APAROffsetCreateDto>>()
			{
				ApiType = ApiType.POST,
				URL = _configService.GetRequestsUrl("invoice/arr"),
				Data = requestsDto
			});

			Guards.ThrowInvalidOperationIf(!createResult.IsSuccess, "Failed to create request" + createResult.Message);

			await _noteService.Create(createResult.Result, notes);
		}

		public async Task Update(bool isUpdatable, long requestId, IEnumerable<APAROffsetAPRowDto> apRows, IEnumerable<APAROffsetARRowDto> arRows, IEnumerable<NoteRowDto> notes)
        {
			if(isUpdatable)
			{
				var requestsDto = ToCreateDto(apRows, arRows);

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

		private static List<APAROffsetCreateDto> ToCreateDto(IEnumerable<APAROffsetAPRowDto> apRows, IEnumerable<APAROffsetARRowDto> arRows)
		{
			var apRequests = apRows.Select(r => new APAROffsetCreateDto()
			{
				InvoiceNumber = r.InvoiceNumber,
				Amount = r.InvoiceAmount,
				Type = "AP",
				InvoiceDate = r.InvoiceDate,
				CustomerName = r.CustomerName,
				CustomerNumber = r.CustomerNumber
			});

			var arRequests = arRows.Select(r => new APAROffsetCreateDto()
			{
				InvoiceNumber = r.InvoiceNumber,
				Amount = r.Amount,
				Type = "AR",
				ReasonCode = r.AdjustmentReason,
				InvoiceDate = DateTime.Now,
				CustomerName = r.CustomerName,
				CustomerNumber = r.CustomerNumber
			});

			return apRequests.Concat(arRequests).ToList();
		}
	}
}
