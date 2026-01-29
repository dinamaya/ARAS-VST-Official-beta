using ARAS.Blazor.App_Code.Globals;
using ARAS.Blazor.App_Code.Globals.Enums;
using ARAS.Blazor.Models.DTOs;
using ARAS.Blazor.Repositories.Interfaces;
using ARAS.Blazor.Services.Interfaces;
using System.Linq;

namespace ARAS.Blazor.Services.Implementations
{
	public class ARInvoiceOffsettingService : IARInvoiceOffsettingService
	{
		private readonly IBaseService _baseService;
		private readonly IConfigService _configService;
		private readonly INoteService _noteService;

		public ARInvoiceOffsettingService(IConfigService configService,
			IBaseService baseService,
			INoteService noteService)
		{
			_baseService = baseService;
			_configService = configService;
			_noteService = noteService;
		}

		private static List<ARInvoiceOffsettingCreateDto> ToCreateDto(
			IEnumerable<ARInvoiceOffsettingRowDto> arRows, 
			IEnumerable<ARInvoiceOffsettingRowDto> cnRows)
		{
			var arRequests = arRows.Select(r => new ARInvoiceOffsettingCreateDto()
			{
				InvoiceAmount = r.InvoiceAmount,
				InvoiceDate = DateTime.Parse(r.InvoiceDate),
				InvoiceNumber = r.InvoiceNumber,
				CustomerName = r.CustomerName,
				CustomerNumber = r.CustomerNumber,
				Type = "AR",
			});

			var cnRequests = cnRows.Select(r => new ARInvoiceOffsettingCreateDto()
			{
				InvoiceAmount = r.InvoiceAmount,
				InvoiceDate = DateTime.Parse(r.InvoiceDate),
				InvoiceNumber = r.InvoiceNumber,
				CustomerName = r.CustomerName,
				CustomerNumber = r.CustomerNumber,
				Type = "CN",
			});

			return cnRequests.Concat(arRequests).ToList();
		}

		public async Task Create(IEnumerable<ARInvoiceOffsettingRowDto> arRows, IEnumerable<ARInvoiceOffsettingRowDto> cnRows, IEnumerable<NoteRowDto> notes)
		{
			var requestsDto = ToCreateDto(arRows, cnRows);

			var createResult = await _baseService.SendAsync<long>(new RequestDto<IEnumerable<ARInvoiceOffsettingCreateDto>>()
			{
				ApiType = ApiType.POST,
				URL = _configService.GetRequestsUrl("invoice/ari"),
				Data = requestsDto
			},
				onSuccessSendCallBack: (resp) =>
				{
					Guards.ThrowInvalidOperationIf(!resp.IsSuccess, "Failed to create request" + resp.Message);
					return Task.CompletedTask;
				}
			);

			await _noteService.Create(createResult.Result, notes);
		}

		public  async Task<IEnumerable<ARInvoiceOffsettingRowDto>> GetAdjustments(long requestId)
        {
			var arResponse = await _baseService.SendAsync<IEnumerable<ARInvoiceOffsettingRowDto>>(new RequestDto()
				{
					URL = _configService.GetAdjustmentsUrl($"ofr/{requestId}"),
				},
				onSuccessSendCallBack: async (resp) =>
				{
					await Task.Run(() =>
					{
						Guards.ThrowInvalidOperationIf(!resp.IsSuccess, "Failed to fetch the AR adjustments");
					});
				}
			);
			return arResponse.Result;
		}

        public async Task Update(long requestId, IEnumerable<ARInvoiceOffsettingRowDto> arRows, IEnumerable<ARInvoiceOffsettingRowDto> cnRows, IEnumerable<NoteRowDto> notes)
		{
			var requestsDto = ToCreateDto(arRows, cnRows);

			var result = await _baseService.SendAsync<long>(new RequestDto<IEnumerable<ARInvoiceOffsettingCreateDto>>()
				{
					ApiType = ApiType.PUT,
					URL = _configService.GetRequestsUrl($"invoice/ari/{requestId}"),
					Data = requestsDto
				},
				onSuccessSendCallBack: (resp) =>
				{
					Guards.ThrowInvalidOperationIf(!resp.IsSuccess, "Failed to update request" + resp.Message);
					return Task.CompletedTask;
				}
			);

			await _noteService.Create(requestId, notes);
		}

	}
}
