using ARAS.Blazor.App_Code.Globals;
using ARAS.Blazor.Models.DTOs;
using ARAS.Blazor.Services.Interfaces;
using Radzen;

namespace ARAS.Blazor.Services.Implementations
{
    public class APAROffsetService :
		BaseAdjustmentCommandService<APAROffsetCreateDto, APAROffsetRowDto, object>,
        IAPAROffsetService
	{
		private readonly IBaseService _baseService;
		private readonly string adjustmentUrl = string.Empty;

		public APAROffsetService(
			IConfigService configService,
			IBaseAdjustmentService<APAROffsetCreateDto, APAROffsetRowDto, object> baseAdjustment,
			IBaseService baseService) :
			base(baseAdjustment, configService.GetAPAROffsetsUrl(), "arr", Map)
		{
			_baseService = baseService;
			adjustmentUrl = configService.GetAdjustmentsUrl();
		}

		public async Task Create(IEnumerable<APAROffsetAPRowDto> apRows, IEnumerable<APAROffsetARRowDto> arRows, IEnumerable<NoteRowDto> notes)
		{
			var requestsDto = ToCreateDto(apRows, arRows);
			await _baseAdjustment.Create(requestsDto, notes, baseUrl);
		}

		public async Task Update(long requestId, IEnumerable<APAROffsetAPRowDto> apRows, IEnumerable<APAROffsetARRowDto> arRows, IEnumerable<NoteRowDto> notes)
        {
			var requestsDto = ToCreateDto(apRows, arRows);
			await _baseAdjustment.Update(requestId, requestsDto, notes, baseUrl);
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
                InvoiceDate = DateTime.UtcNow.ToLocalTime(),
                CustomerName = "",
				CustomerNumber = ""
			});

			return apRequests.Concat(arRequests).ToList();
		}

		private static readonly Func<APAROffsetRowDto, APAROffsetCreateDto> Map = (row) =>
		{
			var aps = row.APGroup.Select(r => new APAROffsetCreateDto()
			{
				InvoiceId = r.InvoiceNumber,
				Amount = r.InvoiceAmount,
				Type = "AP",
				InvoiceDate = r.InvoiceDate,
				CustomerName = r.CustomerName,
				CustomerNumber = r.CustomerNumber
			});

			var ars = row.ARGroup.Select(r => new APAROffsetCreateDto()
			{
				InvoiceId = r.InvoiceNumber,
				Amount = r.Amount,
				Type = "AR",
				ReasonCode = r.AdjustmentReason,
                InvoiceDate = DateTime.UtcNow.ToLocalTime(),
                CustomerName = "",
				CustomerNumber = ""
			});

			return new APAROffsetCreateDto();
		};
	}
}
