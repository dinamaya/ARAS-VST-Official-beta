using ARAS.Main.Oracle.Api.Models.Dtos;

namespace ARAS.Main.Oracle.Api.Repositories.Interfaces
{
	public interface IInvoiceRepository
	{
		Task<InvoiceDetailsDto> GetInvoiceNo(string invoiceNumber);
		Task<IEnumerable<InvoiceDetailsDto>> GetInvoiceDetails(string invoiceNumber);
	}
}
