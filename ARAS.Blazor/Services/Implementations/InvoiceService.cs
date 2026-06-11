using ARAS.Blazor.App_Code.Globals;
using ARAS.Blazor.App_Code.Globals.Constants;
using ARAS.Blazor.Models.DTOs;
using ARAS.Blazor.Services.Interfaces;

namespace ARAS.Blazor.Services.Implementations
{
	public class InvoiceService : IInvoiceService
	{
		private readonly IBaseService _baseService;
		private readonly IConfigService _configService;

		public InvoiceService(IBaseService baseService, IConfigService configService)
		{
			_baseService = baseService;
			_configService = configService;
		}

		// -------------------------------------------------------------------------
		// FIXED: Single-invoice lookup endpoint uses [FromQuery] on the API side.
		// Sending params as JSON body resulted in 400 Bad Request because the
		// required query string params were missing.
		// Now builds the query string and uses RequestDto (no Data property).
		// -------------------------------------------------------------------------
		public async Task<InvoiceDetailsDto> GetDetails(InvoiceDetailsRequestDto request)
		{
			var queryParams = new List<string>();

			if (!string.IsNullOrEmpty(request.InvoiceNumber))
				queryParams.Add($"invoiceNumber={Uri.EscapeDataString(request.InvoiceNumber)}");
			if (!string.IsNullOrEmpty(request.CustomerName))
				queryParams.Add($"customerName={Uri.EscapeDataString(request.CustomerName)}");
			if (!string.IsNullOrEmpty(request.CustomerNumber))
				queryParams.Add($"customerNumber={Uri.EscapeDataString(request.CustomerNumber)}");

			queryParams.Add($"invoiceDate={Uri.EscapeDataString(request.InvoiceDate.ToString("yyyy-MM-dd"))}");
			queryParams.Add($"invoiceAmount={request.InvoiceAmount}");

			var url = _configService.GetOracleSingleInvoiceApiUrl()
				+ "?" + string.Join("&", queryParams);

			var response = await _baseService.SendAsync<InvoiceDetailsDto>(new RequestDto()
			{
				URL = url
			});

			Guards.ThrowNullReferenceIf(response?.Result, response.Message);
			return response.Result;
		}

		public async Task<IEnumerable<InvoiceDetailsDto>> GetDetailsList(SearchRequestDto searchRequest)
		{
			var response = await _baseService.SendAsync<IEnumerable<InvoiceDetailsDto>>(new RequestDto<SearchRequestDto>()
			{
				URL = _configService.GetOracleInvoiceApiUrl(),
				Data = searchRequest
			});

			Guards.ThrowNullReferenceIf(response?.Result, response.Message);
			return response.Result;
		}

		public async Task<IEnumerable<InvoiceDetailsDto>> GetAPDetailsList(SearchRequestDto searchRequest)
		{
			var response = await _baseService.SendAsync<IEnumerable<InvoiceDetailsDto>>(new RequestDto<SearchRequestDto>()
			{
				URL = _configService.GetOracleInvoiceApiUrl("ap"),
				Data = searchRequest
			});

			Guards.ThrowNullReferenceIf(response?.Result, response.Message);
			return response.Result;
		}

		public async Task<InvoiceAPDetailsDto> GetAPDetails(string invoiceNumber)
		{
			var response = await _baseService.SendAsync<InvoiceAPDetailsDto>(new RequestDto()
			{
				URL = _configService.GetOracleInvoiceApiUrl($"ap/no/{invoiceNumber}"),
			});

			Guards.ThrowNullReferenceIf(response?.Result, response.Message);
			return response.Result;
		}

		public async Task<IEnumerable<SearchCNDetailsRowDto>> GetSRAutoNetCNDetails(string invoiceNumber)
		{
			var response = await _baseService.SendAsync<IEnumerable<SearchCNDetailsRowDto>>(new RequestDto()
			{
				URL = _configService.GetOracleInvoiceApiUrl($"sr/{invoiceNumber}"),
			});

			Guards.ThrowNullReferenceIf(response?.Result, response.Message);
			return response.Result;
		}

		public async Task<IEnumerable<SearchCNDetailsRowDto>> GetInvoiceCNDetails(string invoiceNumber)
		{
			var response = await _baseService.SendAsync<IEnumerable<SearchCNDetailsRowDto>>(new RequestDto()
			{
				URL = _configService.GetOracleInvoiceApiUrl($"cn/{invoiceNumber}"),
			});

			Guards.ThrowNullReferenceIf(response?.Result, response.Message);
			return response.Result;
		}
	}
}
