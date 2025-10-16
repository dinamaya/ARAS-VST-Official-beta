using ARAS.Main.SSMS.Api.App_Code.Globals.Constants;
using ARAS.Main.SSMS.Api.Context;
using ARAS.Main.SSMS.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.Models.Entities;
using ARAS.Main.SSMS.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ARAS.Main.SSMS.Api.Repositories.Implementations
{
	public class InvoiceRepository : IInvoiceRepository
	{
		private readonly MainDbContext _context;

		public InvoiceRepository(MainDbContext context)
		{
			_context = context;
		}

		public async Task<long> CreateAsync(InvoiceCreateDto data, string createdBy)
		{
			var date = DateTime.UtcNow;
			var invoice = new Invoice();
			
			invoice.InvoiceNumber = data.InvoiceNumber;
			invoice.InvoiceAmount = data.InvoiceAmount;
			invoice.InvoiceDate = data.InvoiceDate;
			invoice.CustomerName = data.CustomerName;
			invoice.CustomerNumber = data.CustomerNumber;

			invoice.DateCreated = date;
			invoice.DateModified = date;
			invoice.CreatedBy = createdBy;
			invoice.ModifiedBy = createdBy;
			invoice.IsActive = true;

			await _context.Invoices.AddAsync(invoice);
			await _context.SaveChangesAsync();

			return invoice.Id;
		}

		public async Task<Invoice> GetById(long id) =>	
			await _context.Invoices.Where(i => i.Id == id && i.IsActive).FirstOrDefaultAsync() ?? 
			throw new InvalidOperationException(Exceptions.NOTFOUND_TRANSACTION);

		public async Task<bool> IsInvoiceNumberAvailable(string invoiceNumber, string adjustmentTypeCode) =>
			await _context.VwInvoiceNumbers.AnyAsync(inv => 
				inv.InvoiceNumber == invoiceNumber && 
				inv.Code == adjustmentTypeCode &&
				inv.Status != "Declined"
			);
	}
}
