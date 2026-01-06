using ARAS.Blazor.Models.DTOs;

namespace ARAS.Blazor.Services.Interfaces
{
	public interface IInvoiceService
	{
		Task<InvoiceDetailsDto> GetDetails(string invoiceNumber);
		Task<IEnumerable<SearchCNDetailsRowDto>> GetSRAutoNetCNDetails(string invoiceNumber);
		Task<IEnumerable<InvoiceDetailsDto>> GetDetailsList(string invoiceNumber);
        Task<InvoiceAPDetailsDto> GetAPDetails(string invoiceNumber);
    }
}
