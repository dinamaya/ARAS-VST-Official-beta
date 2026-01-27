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

		private static List<ARInvoiceOffsettingCreateDto> ToCreateDto(IEnumerable<ARInvoiceOffsettingRowDto> arRows, IEnumerable<ARInvoiceOffsettingCNDetailsDto> cnRows)
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

		public async Task Create(IEnumerable<ARInvoiceOffsettingRowDto> arRows, IEnumerable<ARInvoiceOffsettingCNDetailsDto> cnRows)
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

		}

		public async Task Update(long requestId, IEnumerable<ARInvoiceOffsettingRowDto> rows, IEnumerable<NoteRowDto> notes)
		{
			throw new NotImplementedException();
		}

	}
}
