using ARAS.Main.Oracle.Api.Models.Dtos;

namespace ARAS.Main.Oracle.Api.Repositories.Interfaces
{
	public interface IInvoiceRepository
	{
        Task<InvoiceAPDetailsDto> GetAPInvoiceNo(string invoiceNumber);

		Task<IEnumerable<InvoiceDetailsDto>> GetInvoiceDetails(SearchRequestDto searchRequest);
		Task<IEnumerable<InvoiceDetailsDto>> GetCnInvoiceDetails(string invoiceNo);

		Task<IEnumerable<SearchCNDetailsRowDto>> GetSRAutoNetCNDetails(string invoiceNumber);
	}
}
