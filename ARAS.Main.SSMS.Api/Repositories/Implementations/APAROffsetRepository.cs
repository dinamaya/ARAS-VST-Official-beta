using ARAS.Main.SSMS.Api.Context;
using ARAS.Main.SSMS.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.Models.Entities;
using ARAS.Main.SSMS.Api.Repositories.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ARAS.Main.SSMS.Api.Repositories.Implementations
{
    public class APAROffsetRepository : IAPAROffsetRepository
    {
		private readonly MainDbContext _context;
		private readonly IAdjustmentRepository _adjustmentRepo;
		private readonly IRequestRepository _requestRepo;
		private readonly ITransactionRepository _transactionRepo;
		private readonly IInvoiceRepository _invoiceRepo;

        public APAROffsetRepository(IAdjustmentRepository adjustmentRepo, IRequestRepository requestRepo, ITransactionRepository transactionRepo, IInvoiceRepository invoiceRepo, MainDbContext context)
        {
            _adjustmentRepo = adjustmentRepo;
            _requestRepo = requestRepo;
            _transactionRepo = transactionRepo;
            _invoiceRepo = invoiceRepo;
            _context = context;
        }

        public async Task<long> Create(RequestCreationDto<IEnumerable<APAROffsetCreateDto>> data, string createdBy)
        {
			await using var dbTransaction = await _context.Database.BeginTransactionAsync();

			try
			{
				IList<InvoiceCreateDto> invoices = [];

				IList<ReasonAdjustmentCreateDto> reasons = [];
				IEnumerable<long> invoiceIds = [];
				IEnumerable<long> reasonInvoiceIds = [];

				IList<APAROffsetCreateDto> adjustments = [];

				string adjustmentTypeId = (await _adjustmentRepo.GetAdjustmentInfoByCode("ARR")).Id;
				var requestId = await _requestRepo.CreateAsync(adjustmentTypeId, createdBy);

				var transaction = new TransactionCreateDto(requestId, "For CNC Approval");
				var transactId = await _transactionRepo.CreateAsync(transaction, createdBy);

				foreach (var _data in data.Model)
				{
					if (!string.IsNullOrEmpty(_data.ReasonCode))
						reasons.Add(new ReasonAdjustmentCreateDto(requestId, _data.ReasonCode, _data.Amount, _data.CustomerNumber, _data.CustomerName));
					else
						invoices.Add(new InvoiceCreateDto(_data.InvoiceNumber, _data.Amount, invoiceDate: DateTime.Now, _data.CustomerNumber, _data.CustomerName));
				}

				invoiceIds = await _invoiceRepo.CreateAsync(invoices, createdBy);
				reasonInvoiceIds = await _invoiceRepo.CreateAsync(reasons, createdBy);

				// Combine the model, invoiceIds and requestIds
				var zippedInvoices = invoiceIds
					.Zip(data.Model, (inv, mod) => new
					{
						invoice = inv,
						model = mod
					});
				var zippedReasons = reasonInvoiceIds
					.Zip(reasons, (inv, mod) => new
					{
						invoice = inv,
						model = mod
					});

				foreach (var result in zippedInvoices)
					adjustments.Add(new APAROffsetCreateDto()
					{
						RequestId = requestId,
						InvoiceNumber = result.invoice.ToString(),
						Amount = result.model.Amount,
						Type = result.model.Type,
						ReasonCode = result.model.ReasonCode ?? string.Empty,
						InvoiceDate = result.model.InvoiceDate,
					});


				foreach (var result in zippedReasons)
					adjustments.Add(new APAROffsetCreateDto()
					{
						RequestId = requestId,
						InvoiceNumber = result.invoice.ToString(),
						Amount = result.model.Amount,
						Type = "AR",
						ReasonCode = result.model.ReasonCode ?? string.Empty,
						InvoiceDate = DateTime.MinValue,
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

        public async Task<long> Update(long requestId, RequestCreationDto<IEnumerable<APAROffsetCreateDto>> data, string modifiedBy)
        {
			await using var dbTransaction = await _context.Database.BeginTransactionAsync();

			try
			{
				IList<InvoiceCreateDto> invoices = [];

				IList<ReasonAdjustmentCreateDto> reasons = [];
				IEnumerable<long> invoiceIds = [];
				IEnumerable<long> reasonInvoiceIds = [];

				IList<APAROffsetCreateDto> adjustments = [];

				string resubmissionStatus = await _requestRepo.GetResubmissionStatus(requestId);

				var transaction = new TransactionCreateDto(requestId, resubmissionStatus);
				var transactId = await _transactionRepo.CreateAsync(transaction, modifiedBy);

				// Deactivates the existing apar offset rows and invoices
				var deactApars = await _context.APAROffsets.Where(a => a.RequestId == requestId).ToListAsync();
				deactApars.ForEach(a => a.IsActive = false);
				var deactAparIds = deactApars.Select(a => a.InvoiceId);

				var deactInvoices = await _context.Invoices
					.Where(i => deactAparIds.Contains(i.Id) && i.IsActive)
					.ToListAsync();

				deactInvoices.ForEach(i => i.IsActive = false);

				await _context.SaveChangesAsync();

				foreach (var _data in data.Model)
				{
					if (!string.IsNullOrEmpty(_data.ReasonCode))
						reasons.Add(new ReasonAdjustmentCreateDto(requestId, _data.ReasonCode, _data.Amount, _data.CustomerNumber, _data.CustomerName));
					else
						invoices.Add(new InvoiceCreateDto(_data.InvoiceNumber, _data.Amount, invoiceDate: DateTime.Now, _data.CustomerNumber, _data.CustomerName));
				}

				// Creates new invoices and apar offsets
				invoiceIds = await _invoiceRepo.CreateAsync(invoices, modifiedBy);
				reasonInvoiceIds = await _invoiceRepo.CreateAsync(reasons, modifiedBy);

				// Combine the model, invoiceIds and requestIds
				var zippedInvoices = invoiceIds
					.Zip(data.Model, (inv, mod) => new
					{
						invoice = inv,
						model = mod
					});
				var zippedReasons = reasonInvoiceIds
					.Zip(reasons, (inv, mod) => new
					{
						invoice = inv,
						model = mod
					});

				foreach (var result in zippedInvoices)
					adjustments.Add(new APAROffsetCreateDto()
					{
						RequestId = requestId,
						InvoiceNumber = result.invoice.ToString(),
						Amount = result.model.Amount,
						Type = result.model.Type,
						ReasonCode = result.model.ReasonCode ?? string.Empty,
						InvoiceDate = result.model.InvoiceDate,
					});

				foreach (var result in zippedReasons)
					adjustments.Add(new APAROffsetCreateDto()
					{
						RequestId = requestId,
						InvoiceNumber = result.invoice.ToString(),
						Amount = result.model.Amount,
						Type = "AR",
						ReasonCode = result.model.ReasonCode ?? string.Empty,
						InvoiceDate = DateTime.MinValue,
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

		public async Task<APAROffsetRowDto> GetAPAdjustmentsByRequestId(long requestId)
		{
			return new APAROffsetRowDto()
			{
				APGroup = await _context.VwAparoffsetRows.AsNoTracking()
					.Where(r => r.Type == "AP" && r.RequestiD == requestId && r.IsActive)
					.Select(r => new APAROffsetAPRowDto()
					{
						Id = r.Id.ToString(),
						InvoiceAmount = r.InvoiceAmount,
						InvoiceNumber = r.InvoiceNumber,
						InvoiceDate = r.InvoiceDate,
						CustomerName = r.CustomerName,
						CustomerNumber = r.CustomerNumber,
					})
				.ToListAsync(),
				ARGroup = await _context.VwAparoffsetRows.AsNoTracking()
					 .Where(r => r.Type == "AR" && r.RequestiD == requestId && r.IsActive)
					 .Select(r => new APAROffsetARRowDto()
					 {
						 Id = r.Id.ToString(),
						 Amount = r.InvoiceAmount,
						 InvoiceNumber = r.InvoiceNumber,
						 AdjustmentReason = r.ReasonCode,
						 RowType = !string.IsNullOrEmpty(r.ReasonCode)
						   ? ARRowType.Adjustment
						  : ARRowType.Invoice,

						 CustomerName = r.CustomerName,
						 CustomerNumber = r.CustomerNumber
					 })
					.ToListAsync()
			};
		}
	}
}
