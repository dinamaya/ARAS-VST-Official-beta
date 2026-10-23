using ARAS.Blazor.Models.DTOs;

namespace ARAS.Blazor.Services.Interfaces
{
	public interface IInvoiceService
	{
		Task<InvoiceDetailsDto> GetDetails(InvoiceDetailsRequestDto request);
		Task<IEnumerable<InvoiceDetailsDto>> GetDetailsList(SearchRequestDto searchRequest);
		Task<IEnumerable<InvoiceDetailsDto>> GetReceiptDetailsList(SearchRequestDto searchRequest);
		Task<IEnumerable<InvoiceDetailsDto>> GetAPDetailsList(SearchRequestDto searchRequest);
		Task<IEnumerable<ApplicableReceiptInvoiceDto>> GetApplicableInvoicesForReceipt(string receiptNumber);

		Task<IEnumerable<SearchCNDetailsRowDto>> GetSRAutoNetCNDetails(string invoiceNumber);
		
		Task<InvoiceAPDetailsDto> GetAPDetails(string invoiceNumber);
    }
}
