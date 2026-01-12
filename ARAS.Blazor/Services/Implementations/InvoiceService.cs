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

		public async Task<InvoiceDetailsDto> GetDetails(string invoiceNumber)
		{
			var response = await _baseService.SendAsync<InvoiceDetailsDto>(new RequestDto()
			{
				URL = _configService.GetOracleInvoiceApiUrl($"no/{invoiceNumber}"),
			});

			Guards.ThrowNullReferenceIf(response?.Result, response.Message);
			return response.Result;
		}

		public async Task<IEnumerable<InvoiceDetailsDto>> GetDetailsList(SearchRequestDto searchRequest)
		{
			var response = await _baseService.SendAsync<IEnumerable<InvoiceDetailsDto>>(new RequestDto<SearchRequestDto>()
			{
				URL = _configService.GetOracleInvoiceApiUrl($"details"),
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
