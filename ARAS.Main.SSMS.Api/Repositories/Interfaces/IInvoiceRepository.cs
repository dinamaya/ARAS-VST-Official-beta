using ARAS.Main.SSMS.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.Models.Entities;

namespace ARAS.Main.SSMS.Api.Repositories.Interfaces
{
	public interface IInvoiceRepository : IReadSingleRepository<Invoice, long>, ICreateRepository<InvoiceCreateDto, long>, ICreateRepository<SRAutoNetInvoiceCreateDto, long>
	{
		Task<bool> IsInvoiceNumberAvailable(string invoiceNumber, string adjustmentTypeCode);
	}
}
