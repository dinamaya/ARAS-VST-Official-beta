using ARAS.Main.Oracle.Api.Models.Dtos;
using Oracle.ManagedDataAccess.Client;

namespace ARAS.Main.Oracle.Api.Repositories.Interfaces
{
	public interface IInvoiceRepository
	{
        Task<InvoiceAPDetailsDto> GetAPInvoiceNo(string invoiceNumber);

		Task<IEnumerable<InvoiceDetailsDto>> GetInvoiceDetails(SearchRequestDto searchRequest);
		Task<IEnumerable<InvoiceDetailsDto>> GetAPInvoiceDetails(SearchRequestDto searchRequest);
		Task<IEnumerable<InvoiceDetailsDto>> GetCnInvoiceDetails(string invoiceNo);
		Task<InvoiceDetailsDto> GetOneInvoiceDetails(InvoiceDetailsRequestDto invoice);

		Task<IEnumerable<SearchCNDetailsRowDto>> GetSRAutoNetCNDetails(string invoiceNumber);
		Task<string> GetCustomerTrxIdByInvoiceDetails(CustomerInvoiceRequestDto invoiceDetails);
		Task<string> GetCustomerTrxIdByInvoiceDetails(OracleConnection oracleConnection, CustomerInvoiceRequestDto invoiceDetails);
	}
}
