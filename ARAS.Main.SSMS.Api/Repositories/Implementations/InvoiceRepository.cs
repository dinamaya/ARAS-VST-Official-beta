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
			var date = DateTime.Now;
            var invoice = new Invoice
            {
                InvoiceNumber = data.InvoiceNumber,
                InvoiceAmount = data.InvoiceAmount,
                InvoiceDate = data.InvoiceDate,
                CustomerName = data.CustomerName,
                CustomerNumber = data.CustomerNumber,

                DateCreated = date,
                DateModified = date,
                CreatedBy = createdBy,
                ModifiedBy = createdBy,
                IsActive = true
            };

            await _context.Invoices.AddAsync(invoice);
			await _context.SaveChangesAsync();

			return invoice.Id;
		}


		public async Task<IEnumerable<long>> CreateAsync(IEnumerable<InvoiceCreateDto> data, string createdBy)
		{
			var date = DateTime.Now;
			IList<Invoice> results = [];

			foreach (var _data in data)
			{
				results.Add(new Invoice
				{
					InvoiceNumber = _data.InvoiceNumber,
					InvoiceAmount = _data.InvoiceAmount,
					InvoiceDate = _data.InvoiceDate,
					CustomerName = _data.CustomerName,
					CustomerNumber = _data.CustomerNumber,

					DateCreated = date,
					DateModified = date,
					CreatedBy = createdBy,
					ModifiedBy = createdBy,
					IsActive = true
				});
			}

			await _context.Invoices.AddRangeAsync(results);
			await _context.SaveChangesAsync();

			return results.Select(r => r.Id);
		}

		public async Task<IEnumerable<long>> CreateAsync(IEnumerable<ReasonAdjustmentCreateDto> data, string createdBy)
		{
			var date = DateTime.Now;
			IList<Invoice> results = [];

			foreach (var _data in data)
			{
				results.Add(new Invoice
				{
					InvoiceNumber = "",
					InvoiceAmount = 0d,
					InvoiceDate = DateTime.MinValue,
					CustomerName = _data.CustomerName,
					CustomerNumber = _data.CustomerNumber,

					DateCreated = date,
					DateModified = date,
					CreatedBy = createdBy,
					ModifiedBy = createdBy,
					IsActive = true
				});
			}

			await _context.Invoices.AddRangeAsync(results);
			await _context.SaveChangesAsync();

			return results.Select(r => r.Id);
		}

		public async Task<long> CreateAsync(SRAutoNetInvoiceCreateDto data, string createdBy)
		{
			var date = DateTime.Now;
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

			IList<CNDetails> cnDetails = [];

			foreach(var remark in data.Remarks)
			{
				var _cNDetails = new CNDetails()
				{
					InvoiceId = invoice.Id,
					CNRef = remark.CNRef,
					//CNAMT = remark.CNAmt,
					WT = remark.WT,

					CreatedBy = createdBy,
					DateCreated = date,
					ModifiedBy = createdBy,
					DateModified = date,
					IsActive = true
				};

				cnDetails.Add(_cNDetails);
			}

			await _context.CNDetails.AddRangeAsync(cnDetails);
			await _context.SaveChangesAsync();

			return invoice.Id;
		}

		public async Task<Invoice> GetById(long id) =>	
			await _context.Invoices.AsNoTracking().Where(i => i.Id == id && i.IsActive).FirstOrDefaultAsync() ?? 
			throw new InvalidOperationException(Exceptions.NOTFOUND_TRANSACTION);

		public async Task<bool> IsInvoiceNumberAvailable(string invoiceNumber, string adjustmentTypeCode)
		{
			string[] invalidStatuses = ["Rejected", "Declined"];
			return !(await _context.VwInvoiceNumbers.AnyAsync(inv =>
				inv.InvoiceNumber == invoiceNumber &&
				inv.Code == adjustmentTypeCode &&
				!invalidStatuses.Contains(inv.Status)
			));
		}
	}
}
