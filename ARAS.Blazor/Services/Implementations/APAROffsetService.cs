using ARAS.Blazor.App_Code.Globals;
using ARAS.Blazor.Models.DTOs;
using ARAS.Blazor.Services.Interfaces;
using Radzen;

namespace ARAS.Blazor.Services.Implementations
{
    public class APAROffsetService
	{
		private readonly IBaseService _baseService;
		private readonly string adjustmentUrl = string.Empty;

		public APAROffsetService(
			IConfigService configService,
			IBaseService baseService)
		{
			_baseService = baseService;
			adjustmentUrl = configService.GetAdjustmentsUrl();
		}

		public async Task Create(IEnumerable<APAROffsetAPRowDto> apRows, IEnumerable<APAROffsetARRowDto> arRows, IEnumerable<NoteRowDto> notes)
		{
			var requestsDto = ToCreateDto(apRows, arRows);
		}

		public async Task Update(long requestId, IEnumerable<APAROffsetAPRowDto> apRows, IEnumerable<APAROffsetARRowDto> arRows, IEnumerable<NoteRowDto> notes)
        {
			var requestsDto = ToCreateDto(apRows, arRows);
		}

		public async Task<APAROffsetRowDto> GetAdjustments(long requestId)
		{
			var arResponse = await _baseService.SendAsync<APAROffsetRowDto>(new RequestDto()
				{
					URL = $"{adjustmentUrl}aar/{requestId}",
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
				InvoiceId = r.InvoiceNumber,
				Amount = r.InvoiceAmount,
				Type = "AP",
				InvoiceDate = r.InvoiceDate,
				CustomerName = r.CustomerName,
				CustomerNumber = r.CustomerNumber
			});

			var arRequests = arRows.Select(r => new APAROffsetCreateDto()
			{
				InvoiceId = r.InvoiceNumber,
				Amount = r.Amount,
				Type = "AR",
				ReasonCode = r.AdjustmentReason,
				InvoiceDate = DateTime.Now,
				CustomerName = "",
				CustomerNumber = ""
			});

			return apRequests.Concat(arRequests).ToList();
		}
	}
}
