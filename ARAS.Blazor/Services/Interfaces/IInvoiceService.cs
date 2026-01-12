using ARAS.Blazor.Models.DTOs;

namespace ARAS.Blazor.Services.Interfaces
{
	public interface IInvoiceService
	{
		Task<InvoiceDetailsDto> GetDetails(string invoiceNumber);
		Task<IEnumerable<InvoiceDetailsDto>> GetDetailsList(SearchRequestDto searchRequest);

		Task<IEnumerable<SearchCNDetailsRowDto>> GetSRAutoNetCNDetails(string invoiceNumber);
		
		Task<InvoiceAPDetailsDto> GetAPDetails(string invoiceNumber);
    }
}
