using ARAS.Main.SSMS.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.Models.Entities;

namespace ARAS.Main.SSMS.Api.Repositories.Interfaces
{
	public interface IInvoiceRepository : IReadSingleRepository<Invoice, long>, ICreateRepository<InvoiceCreateDto, long>
	{
		Task<bool> IsInvoiceNumberAvailable(string invoiceNumber, string adjustmentTypeCode);
	}
}
