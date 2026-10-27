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
		private readonly ILogger<APAROffsetRepository> _logger;

        public APAROffsetRepository(IAdjustmentRepository adjustmentRepo, IRequestRepository requestRepo, ITransactionRepository transactionRepo, IInvoiceRepository invoiceRepo, MainDbContext context, ILogger<APAROffsetRepository> logger)
        {
            _adjustmentRepo = adjustmentRepo;
            _requestRepo = requestRepo;
            _transactionRepo = transactionRepo;
            _invoiceRepo = invoiceRepo;
            _context = context;
			_logger = logger;
        }

        public async Task<long> Create(RequestCreationDto<IEnumerable<APAROffsetCreateDto>> data, string createdBy)
        {
			await using var dbTransaction = await _context.Database.BeginTransactionAsync();

			try
			{
				IList<InvoiceCreateDto> invoices = [];
				IEnumerable<long> invoiceIds = [];

				IList<APAROffsetCreateDto> adjustments = [];

				string adjustmentTypeId = (await _adjustmentRepo.GetAdjustmentInfoByCode("ARR")).Id;
				var requestId = await _requestRepo.CreateAsync(adjustmentTypeId, createdBy);

				var transaction = new TransactionCreateDto(requestId, "For CNC Approval");
				var transactId = await _transactionRepo.CreateAsync(transaction, createdBy);

				foreach (var _data in data.Model)
					invoices.Add(new InvoiceCreateDto(_data.InvoiceNumber, _data.InvoiceAmount, invoiceDate: DateTime.Now, _data.CustomerNumber, _data.CustomerName, _data.CustomerTrxId, _data.PaymentScheduleId, _data.CurrencyCode, _data.GlAccountingDate));

				invoiceIds = await _invoiceRepo.CreateAsync(invoices, createdBy);

				// Combine the model, invoiceIds and requestIds
				var zippedInvoices = invoiceIds
					.Zip(data.Model, (inv, mod) => new
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
				IEnumerable<long> invoiceIds = [];

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
					invoices.Add(new InvoiceCreateDto(_data.InvoiceNumber, _data.InvoiceAmount, invoiceDate: DateTime.Now, _data.CustomerNumber, _data.CustomerName, _data.CustomerTrxId, _data.PaymentScheduleId, _data.CurrencyCode, _data.GlAccountingDate));

				// Creates new invoices and apar offsets
				invoiceIds = await _invoiceRepo.CreateAsync(invoices, modifiedBy);

				// Combine the model, invoiceIds and requestIds
				var zippedInvoices = invoiceIds
					.Zip(data.Model, (inv, mod) => new
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
				APGroup = await (
					from apar in _context.APAROffsets.AsNoTracking()
					join invoice in _context.Invoices.AsNoTracking() on apar.InvoiceId equals invoice.Id
					where apar.Type == "AP"
					   && apar.RequestId == requestId
					   && apar.IsActive
					   && invoice.IsActive
					select new APAROffsetAPRowDto
					{
						Id = apar.Id.ToString(),
						InvoiceAmount = invoice.InvoiceAmount,
						InvoiceNumber = invoice.InvoiceNumber,
						InvoiceDate = invoice.InvoiceDate,
						CustomerName = invoice.CustomerName,
						CustomerNumber = invoice.CustomerNumber,
					}
				).ToListAsync(),
				ARGroup = await (
					from apar in _context.APAROffsets.AsNoTracking()
					join invoice in _context.Invoices.AsNoTracking() on apar.InvoiceId equals invoice.Id
					where apar.Type == "AR"
					   && apar.RequestId == requestId
					   && apar.IsActive
					   && invoice.IsActive
					select new APAROffsetARRowDto
					 {
						 Id = apar.Id.ToString(),
						 Amount = apar.Amount,
						 InvoiceAmount = invoice.InvoiceAmount,
						 InvoiceNumber = invoice.InvoiceNumber,
						 AdjustmentReason = apar.ReasonCode,
						 RowType = !string.IsNullOrEmpty(apar.ReasonCode)
						   ? ARRowType.Adjustment
						  : ARRowType.Invoice,

						 CustomerName = invoice.CustomerName,
						 CustomerNumber = invoice.CustomerNumber,
						 CustomerTrxId = invoice.CustomerTrxId,
						 PaymentScheduleId = invoice.PaymentScheduleId,
						 CurrencyCode = invoice.CurrencyCode,
						GlAccountingDate = invoice.GlAccountingDate
					}
				).ToListAsync()
			};
		}

		public async Task<IEnumerable<AdjustmentPostingDto>> GetStagingData(long requestId)
		{
			var rows = await (
				from apar in _context.APAROffsets.AsNoTracking()
				join invoice in _context.Invoices.AsNoTracking() on apar.InvoiceId equals invoice.Id
				where apar.RequestId == requestId && apar.IsActive && invoice.IsActive && apar.Type == "AR"
				orderby apar.Id
				select new AdjustmentPostingDto
				{
					HeaderId = apar.Id,
					AdjustmentId = apar.Id,
					InvoiceNumber = invoice.InvoiceNumber,
					CustomerTrxId = invoice.CustomerTrxId,
					InvoiceAmount = invoice.InvoiceAmount,
					AdjustmentAmount = apar.Amount,
					InvoiceDate = invoice.InvoiceDate,
					PaymentScheduleId = invoice.PaymentScheduleId,
					CurrencyCode = invoice.CurrencyCode,
					GlAccountingDate = invoice.GlAccountingDate,
					DateApplied = DateTime.Now,
					AdjustmentActivity = string.IsNullOrWhiteSpace(apar.ReasonCode)
						? "AP/AR Adjustment"
						: "Write-Off Small Amount - INV",
					ReasonCode = apar.ReasonCode ?? string.Empty,
					Remarks = string.Empty,
					CustomerName = invoice.CustomerName,
					CustomerNumber = invoice.CustomerNumber
				}).ToListAsync();

			// The reconciliation screen shows the full AR invoice and the AP total,
			// but Fusion must receive only the portion of AR that is actually offset.
			// Otherwise the full AR amount is posted as an offset and the write-off is
			// posted on top of it, which increases the invoice instead of netting it.
			var apTotal = await _context.APAROffsets.AsNoTracking()
				.Where(a => a.RequestId == requestId && a.IsActive && a.Type == "AP")
				.SumAsync(a => (double?)a.Amount) ?? 0d;

			var remainingApToOffset = Math.Max(apTotal, 0d);
			foreach (var row in rows.Where(r => string.IsNullOrWhiteSpace(r.ReasonCode)))
			{
				var arBalance = Math.Max(row.AdjustmentAmount, 0d);
				var offsetAmount = Math.Min(arBalance, remainingApToOffset);
				row.AdjustmentAmount = offsetAmount;
				remainingApToOffset -= offsetAmount;
			}

			_logger.LogInformation(
				"Prepared AP/AR posting for request {RequestId}: AP total {ApTotal}, offset amount {OffsetAmount}, remaining AP {RemainingApToOffset}",
				requestId,
				apTotal,
				rows.Where(r => string.IsNullOrWhiteSpace(r.ReasonCode)).Sum(r => r.AdjustmentAmount),
				remainingApToOffset);

			var arInvoiceContext = rows.FirstOrDefault(r =>
				string.IsNullOrWhiteSpace(r.ReasonCode) &&
				r.CustomerTrxId.HasValue);

			if (arInvoiceContext is not null)
			{
				foreach (var row in rows.Where(r => !string.IsNullOrWhiteSpace(r.ReasonCode)))
				{
					row.CustomerTrxId ??= arInvoiceContext.CustomerTrxId;
					row.PaymentScheduleId ??= arInvoiceContext.PaymentScheduleId;
					row.CurrencyCode ??= arInvoiceContext.CurrencyCode;
					row.GlAccountingDate ??= arInvoiceContext.GlAccountingDate;
				}
			}

			return rows;
		}
	}
}
