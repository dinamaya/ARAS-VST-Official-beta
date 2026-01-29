using ARAS.Main.SSMS.Api.App_Code.Globals.Constants;
using ARAS.Main.SSMS.Api.Context;
using ARAS.Main.SSMS.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.Models.SQLVIews;
using ARAS.Main.SSMS.Api.Repositories.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace ARAS.Main.SSMS.Api.Repositories.Implementations
{
    public class AROffsettingRepository : IAROffsettingRepository
    {
        private readonly MainDbContext _context;
        private readonly IAdjustmentRepository _adjustmentRepo;
        private readonly IRequestRepository _requestRepo;
        private readonly ITransactionRepository _transactionRepo;
        private readonly IInvoiceRepository _invoiceRepo;

        public AROffsettingRepository(IAdjustmentRepository adjustmentRepo, IRequestRepository requestRepo, ITransactionRepository transactionRepo, IInvoiceRepository invoiceRepo, MainDbContext context)
        {
            _adjustmentRepo = adjustmentRepo;
            _requestRepo = requestRepo;
            _transactionRepo = transactionRepo;
            _invoiceRepo = invoiceRepo;
            _context = context;
        }

        public async Task<long> Create(RequestCreationDto<IEnumerable<ARInvoiceOffsettingCreateDto>> data, string createdBy)
        {
            await using var dbTransaction = await _context.Database.BeginTransactionAsync();

            try
            {
                IList<InvoiceCreateDto> invoices = [];

                IEnumerable<long> invoiceIds = [];

                IList<ARIAdjustmentCreateDto> adjustments = [];

                string adjustmentTypeId = (await _adjustmentRepo.GetAdjustmentInfoByCode("OFR")).Id;
                var requestId = await _requestRepo.CreateAsync(adjustmentTypeId, createdBy);

                var transaction = new TransactionCreateDto(requestId, "Pending");
                var transactId = await _transactionRepo.CreateAsync(transaction, createdBy);

                foreach (var _data in data.Model)
                    invoices.Add(new InvoiceCreateDto(_data.InvoiceNumber, _data.InvoiceAmount, invoiceDate: DateTime.Now, _data.CustomerNumber, _data.CustomerName));
                
                invoiceIds = await _invoiceRepo.CreateAsync(invoices, createdBy);

                // Combine the model, invoiceIds and requestIds
                var zippedInvoices = invoiceIds
                    .Zip(data.Model, (inv, mod) => new
                    {
                        invoice = inv,
                        model = mod
                    });

                foreach (var result in zippedInvoices)
                    adjustments.Add(new ARIAdjustmentCreateDto()
                    {
                        RequestId = requestId,
                        InvoiceNumber = result.invoice,
                        AdjustmentAmount = result.model.InvoiceAmount,
                        Type = result.model.Type
                    });

                await _adjustmentRepo.CreateAsync(adjustments, createdBy);
                await dbTransaction.CommitAsync();

                return requestId;
            }
            catch
            {
                await dbTransaction.RollbackAsync();
                throw;
            }
        }

        public async Task<long> Update(long requestId, RequestCreationDto<IEnumerable<ARInvoiceOffsettingCreateDto>> data, string modifiedBy)
		{
			await using var dbTransaction = await _context.Database.BeginTransactionAsync();

			try
			{
				IList<InvoiceCreateDto> invoices = [];

				IEnumerable<long> invoiceIds = [];

				IList<ARIAdjustmentCreateDto> adjustments = [];

				var transaction = new TransactionCreateDto(requestId, "Resubmitted");
				var transactId = await _transactionRepo.CreateAsync(transaction, modifiedBy);

				// Deactivates the existing AR offset rows and invoices
				var deactArs = await _context.AROffsets.Where(a => a.RequestId == requestId).ToListAsync();
				deactArs.ForEach(a => a.IsActive = false);
				var deactAparIds = deactArs.Select(a => a.InvoiceId);

				var deactInvoices = await _context.Invoices
					.Where(i => deactAparIds.Contains(i.Id) && i.IsActive)
					.ToListAsync();

				deactInvoices.ForEach(i => i.IsActive = false);

				await _context.SaveChangesAsync();

				foreach (var _data in data.Model)
					invoices.Add(new InvoiceCreateDto(_data.InvoiceNumber, _data.InvoiceAmount, invoiceDate: DateTime.Now, _data.CustomerNumber, _data.CustomerName));

				invoiceIds = await _invoiceRepo.CreateAsync(invoices, modifiedBy);

				// Combine the model, invoiceIds and requestIds
				var zippedInvoices = invoiceIds
					.Zip(data.Model, (inv, mod) => new
					{
						invoice = inv,
						model = mod
					});

				foreach (var result in zippedInvoices)
					adjustments.Add(new ARIAdjustmentCreateDto()
					{
						RequestId = requestId,
						InvoiceNumber = result.invoice,
						AdjustmentAmount = result.model.InvoiceAmount,
						Type = result.model.Type
					});

				await _adjustmentRepo.CreateAsync(adjustments, modifiedBy);
				await dbTransaction.CommitAsync();

				return requestId;
			}
			catch
			{
				await dbTransaction.RollbackAsync();
				throw;
			}
		}

		public async Task<IEnumerable<ARInvoiceOffsettingRowDto>> GetAdjustmentsByRequestId(long requestId)
		{
			return await _context.VwAradjustmentsVw.Where(a => a.RequestId == requestId).Select(a =>
			new ARInvoiceOffsettingRowDto()
			{
				Id = a.CreatorId.ToString(),
				AdjustmentActivity = a.AdjustmentType,
				InvoiceAmount = a.InvoiceAmount,
				InvoiceDate = a.InvoiceDate.ToString(Formats.Date.DISPLAY),
				InvoiceNumber = a.InvoiceNumber,
				CustomerName = a.CustomerName,
				CustomerNumber = a.CustomerNumber,
				Type = a.InvoiceType
			}).ToListAsync();
		}
	}
}
