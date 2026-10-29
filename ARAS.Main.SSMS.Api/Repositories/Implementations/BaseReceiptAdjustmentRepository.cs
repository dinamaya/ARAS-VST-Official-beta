using ARAS.Main.SSMS.Api.App_Code.Globals;
using ARAS.Main.SSMS.Api.App_Code.Globals.Helpers;
using ARAS.Main.SSMS.Api.Context;
using ARAS.Main.SSMS.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.Models.Entities;
using ARAS.Main.SSMS.Api.Repositories.Interfaces;
using ARAS.Main.SSMS.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ARAS.Main.SSMS.Api.Repositories.Implementations
{
    public class BaseReceiptAdjustmentRepository : IBaseReceiptAdjustmentRepository
	{
		private readonly MainDbContext _context;
		private readonly IAdjustmentRepository _adjustmentRepo;
		private readonly IRequestRepository _requestRepo;
		private readonly ITransactionRepository _transactionRepo;
		private readonly IInvoiceRepository _invoiceRepo;
		private readonly INoteService _notesService;

        public BaseReceiptAdjustmentRepository(
            MainDbContext context,
            IBackgroundJobService bgJobService,
            IAdjustmentRepository adjustmentRepo,
            IRequestRepository requestRepo,
            ITransactionRepository transactionRepo,
            IRemarksRepository remarksRepo,
            IInvoiceRepository invoiceRepo,
            INoteService notesService)
        {
            _context = context;
            _adjustmentRepo = adjustmentRepo;
            _requestRepo = requestRepo;
            _transactionRepo = transactionRepo;
            _invoiceRepo = invoiceRepo;
            _notesService = notesService;
        }

        public async Task<IEnumerable<ReceiptAdjustmentCreateResponseDto>> Create(RequestCreationDto<IEnumerable<BaseReceiptAdjustmentCreateDto>> data, string createdBy)
		{
			await using var dbTransaction = await _context.Database.BeginTransactionAsync();

			try
			{
				var models = data.Model.ToList();
				string invoiceNumber = models.Select(d => d.InvoiceNumber).First();
				IList<string> adjustmentTypeIds = [];
				IEnumerable<long> requestIds = [];

				IList<TransactionCreateDto> transactions = [];
				IEnumerable<long> transactionIds = [];

				IList<InvoiceCreateDto> invoices = [];
				IEnumerable<long> invoiceIds = [];

				IList<AdjustmentCreateDto> adjustments = [];

				foreach (string adjustmentName in models.Select(d => d.AdjustmentType))
				{
					string adjustmentType = (await _adjustmentRepo.GetAdjustmentInfoByName(adjustmentName)).Id;
					adjustmentTypeIds.Add(adjustmentType);
				}

				string sourceType = models.Select(model => model.SourceType).FirstOrDefault() ?? "Receipt";
				requestIds = await _requestRepo.CreateAsync(adjustmentTypeIds, createdBy, sourceType);
				var requestIdList = requestIds.ToList();
				
				string existingAdjustments = await _requestRepo.InvoiceExistingAdjustments(invoiceNumber, adjustmentTypeIds);

				var sourceLabel = string.Equals(sourceType, "Receipt", StringComparison.OrdinalIgnoreCase)
					? "Receipt Number"
					: "Invoice Number";
				Guards.ThrowInvalidOperationIf(
					!string.IsNullOrEmpty(existingAdjustments),
					$"{sourceLabel} already has the following adjustments:\n{existingAdjustments}");

				foreach (var requestId in requestIdList)
					transactions.Add(new TransactionCreateDto(requestId, "For CNC Approval"));
				transactionIds = await _transactionRepo.CreateAsync(transactions, createdBy);

				foreach (var invoice in models)
					invoices.Add(new InvoiceCreateDto(invoice));
				invoiceIds = await _invoiceRepo.CreateAsync(invoices, createdBy);

				// Combine the model, invoiceIds and requestIds
				var zipped = invoiceIds
					.Zip(models, (inv, mod) => new
					{
						invoice = inv,
						model = mod
					})
					.Zip(requestIdList, (mod, req) => new
						{
							ids = new 
							{
								invoice = mod.invoice,
								requests = req
							},
							model = mod.model
						});

				foreach (var result in zipped)
					adjustments.Add(new AdjustmentCreateDto(
						result.ids.invoice, 
						result.ids.requests,
						result.model)
					);

				await _adjustmentRepo.CreateAsync(adjustments, createdBy);

				var receiptApplications = zipped
					.Where(z => z.model.ReceiptApplications is not null && z.model.ReceiptApplications.Any())
					.SelectMany(z => z.model.ReceiptApplications
						.Where(a => a is not null && a.PaymentScheduleId > 0 && a.AppliedAmount > 0)
						.Select(a => new ReceiptInvoiceApplication
						{
							RequestId = z.ids.requests,
							ReceiptNumber = z.model.InvoiceNumber,
							ReceiptId = z.model.ReceiptId,
							InvoiceId = a.InvoiceId,
							InvoiceNumber = a.InvoiceNumber,
							PaymentScheduleId = a.PaymentScheduleId,
							AppliedAmount = a.AppliedAmount,
							DiscountAmount = a.DiscountAmount,
							AmountDue = a.AmountDue,
							CurrencyCode = a.CurrencyCode,
							CreatedBy = createdBy,
							DateCreated = DateTime.Now,
							ModifiedBy = createdBy,
							DateModified = DateTime.Now,
							IsActive = true
						}))
					.ToList();

				// Persist the requestor's Apply Receipt choice on the request. A request that
				// asks for Apply Receipt must carry at least one valid invoice application;
				// otherwise reject the submission instead of silently posting only the write-off later.
				var applyReceiptRequestIds = zipped
					.Where(z => z.model.ApplyReceiptEnabled
						|| (z.model.ReceiptApplications?.Any(a => a is not null && a.PaymentScheduleId > 0 && a.AppliedAmount > 0) ?? false))
					.Select(z => z.ids.requests)
					.Distinct()
					.ToList();

				foreach (var requestId in applyReceiptRequestIds)
				{
					Guards.ThrowInvalidOperationIf(
						!receiptApplications.Any(a => a.RequestId == requestId),
						"Apply Receipt is enabled but no invoice with a positive applied amount was selected.");
					Guards.ThrowInvalidOperationIf(
						receiptApplications.Any(a => a.RequestId == requestId && (a.ReceiptId is null || a.ReceiptId <= 0)),
						"Apply Receipt is enabled but the receipt ID is missing. Please search the receipt again and resubmit.");
				}

				if (applyReceiptRequestIds.Any())
				{
					var applyReceiptRequests = await _context.Requests
						.Where(r => applyReceiptRequestIds.Contains(r.Id))
						.ToListAsync();

					foreach (var request in applyReceiptRequests)
						request.ApplyReceiptEnabled = true;
				}

				if (receiptApplications.Any())
					await _context.ReceiptInvoiceApplications.AddRangeAsync(receiptApplications);

				// Persist the selected Apply Receipt invoices before committing the transaction.
				// AddRangeAsync only tracks the entities; without SaveChangesAsync the table remains empty.
				await _context.SaveChangesAsync();
				await dbTransaction.CommitAsync();

				var zipResults = requestIdList.Zip(models, 
					(req, mod)	=> new ReceiptAdjustmentCreateResponseDto
					{
						RequestId = req,
						AdjustmentTypeCode = mod.AdjustmentType
					}
				);

				return zipResults;
			}
			catch
			{
				await dbTransaction.RollbackAsync();
				throw;
			}
		}

		public async Task<long> UpdateAsync(long requestId, ReceiptAdjustmentUpdateRequestDto data, string modifiedBy)
		{
			await using var dbTransaction = await _context.Database.BeginTransactionAsync();

			try
			{
				string resubmissionStatus = await _requestRepo.GetResubmissionStatus(requestId);

				var transaction = new TransactionCreateDto(requestId, resubmissionStatus);
				var transactId = await _transactionRepo.CreateAsync(transaction, modifiedBy);

				var adjustmentUpdate = new ReceiptAdjustmentUpdateRequestDto()
				{
					AdjustmentAmount = data.AdjustmentAmount,
					Remarks = data.Remarks
				};

				var adjustment = await _adjustmentRepo.UpdateAsync(requestId, adjustmentUpdate, modifiedBy);

				await dbTransaction.CommitAsync();
				return requestId;
			}
			catch
			{
				await dbTransaction.RollbackAsync();
				throw;
			}
		}

		public async Task<ReceiptAdjustmentUpdateResponseDto> GetDetailsById(long requestId)
		{
			var adjustment = await _context.Adjustments
				.Where(a => a.RequestId == requestId && a.IsActive)
				.Select(a => new
				{
					a.RequestId,
					a.InvoiceId,
					a.AdjustmentAmount,
					a.Remarks,
					SourceType = a.Request.SourceType ?? a.Request.AdjustmentType.Category
				}).FirstOrDefaultAsync()
				?? throw new InvalidOperationException("Failed to fetch the active adjustment details.");
			var invoiceEntity = await _invoiceRepo.GetById(adjustment.InvoiceId);
			var invoice = new InvoiceDetailsRequestDto
			{
				InvoiceNumber = invoiceEntity.InvoiceNumber,
				InvoiceAmount = invoiceEntity.InvoiceAmount,
				InvoiceBalance = invoiceEntity.InvoiceBalance,
				CustomerTrxId = invoiceEntity.CustomerTrxId,
				PaymentScheduleId = invoiceEntity.PaymentScheduleId,
				InvoiceDate = DateOnly.FromDateTime(invoiceEntity.InvoiceDate),
				CustomerName = invoiceEntity.CustomerName,
					CustomerNumber = invoiceEntity.CustomerNumber,
					SourceType = adjustment.SourceType
			};

			var transactionHistories = await _transactionRepo.GetHistoryByRequestId(requestId);
			var notes = await _notesService.GetByRequestId(requestId);
			var receiptApplications = await _context.ReceiptInvoiceApplications
				.AsNoTracking()
				.Where(a => a.RequestId == requestId && a.IsActive)
				.Select(a => new ReceiptInvoiceApplicationPostingDto
				{
					InvoiceId = a.InvoiceId,
					InvoiceNumber = a.InvoiceNumber,
					PaymentScheduleId = a.PaymentScheduleId,
					AppliedAmount = a.AppliedAmount,
					DiscountAmount = a.DiscountAmount,
					AmountDue = a.AmountDue,
					CurrencyCode = a.CurrencyCode
				})
				.ToListAsync();

			var applyReceiptEnabled = await _context.Requests
				.AsNoTracking()
				.Where(r => r.Id == requestId)
				.Select(r => r.ApplyReceiptEnabled)
				.FirstOrDefaultAsync();

			return new ReceiptAdjustmentUpdateResponseDto
			{
				AdjustmentAmount = adjustment.AdjustmentAmount,
				Remarks = adjustment.Remarks,
				Invoice = invoice,
				ApplyReceiptEnabled = applyReceiptEnabled || receiptApplications.Count > 0,
				ReceiptApplications = receiptApplications,
				TransactionHistories = transactionHistories,
				Notes = notes
			};
		}

        public async Task<IEnumerable<AdjustmentPostingDto>> GetStagingDataByRequestId(long requestId)
        {
            var rows = await _context.VwStagingRequestAdjustment
				.Where(a => a.RequestId == requestId)
				.Select(a => new AdjustmentPostingDto
				{
                    HeaderId = StagingHeaderIdHelper.Generate(a.RequestId, a.AdjustmentId),
                    AdjustmentId = a.AdjustmentId,
                    InvoiceNumber = a.InvoiceNumber,
					CustomerTrxId = a.CustomerTrxId,
					AdjustmentAmount = a.AdjustmentAmount,
					InvoiceDate = a.InvoiceDate,
					PaymentScheduleId = a.PaymentScheduleId,
					DateApplied = DateTime.Now,
					AdjustmentActivity = a.Activity,
					ReasonCode = a.ReasonCode,
					Remarks = a.Remarks,
					CustomerName = a.CustomerName,
					CustomerNumber = a.CustomerNumber
				}).ToListAsync();

			rows = await RestoreInvoiceIdentifiersAsync(rows);
			return await AttachReceiptApplicationsAsync(rows);
		}

		public async Task<IEnumerable<AdjustmentPostingDto>> GetStagingDataByRequestIds(IEnumerable<long> requestIds)
		{
            var rows = await _context.VwStagingRequestAdjustment
			   .Where(a => requestIds.Contains(a.RequestId))
			   .Select(a => new AdjustmentPostingDto
			   {
                   HeaderId = StagingHeaderIdHelper.Generate(a.RequestId, a.AdjustmentId),
                   AdjustmentId = a.AdjustmentId,
                   InvoiceNumber = a.InvoiceNumber,
				   CustomerTrxId = a.CustomerTrxId,
				   AdjustmentAmount = a.AdjustmentAmount,
				   InvoiceDate = a.InvoiceDate,
				   PaymentScheduleId = a.PaymentScheduleId,
				   DateApplied = DateTime.Now,
				   AdjustmentActivity = a.Activity,
				   ReasonCode = a.ReasonCode,
				   Remarks = a.Remarks,
				   CustomerName = a.CustomerName,
				   CustomerNumber = a.CustomerNumber
			   }).ToListAsync();

			rows = await RestoreInvoiceIdentifiersAsync(rows);
			return await AttachReceiptApplicationsAsync(rows);
		}

        private async Task<List<AdjustmentPostingDto>> RestoreInvoiceIdentifiersAsync(List<AdjustmentPostingDto> rows)
        {
            var adjustmentIds = rows
                .Where(row => row.CustomerTrxId is null || row.PaymentScheduleId is null || row.InvoiceAmount <= 0 || row.GlAccountingDate is null)
                .Select(row => row.AdjustmentId)
                .Distinct()
                .ToList();

            if (!adjustmentIds.Any())
                return rows;

            var invoiceIdentifiers = await _context.Adjustments
                .Where(adjustment => adjustmentIds.Contains(adjustment.Id))
                .Join(
                    _context.Invoices,
                    adjustment => adjustment.InvoiceId,
                    invoice => invoice.Id,
                    (adjustment, invoice) => new
                    {
                        AdjustmentId = adjustment.Id,
                        invoice.CustomerTrxId,
                        invoice.PaymentScheduleId,
                        invoice.InvoiceAmount,
                        invoice.GlAccountingDate
                    })
                .ToDictionaryAsync(x => x.AdjustmentId);

            foreach (var row in rows)
            {
                if (!invoiceIdentifiers.TryGetValue(row.AdjustmentId, out var identifiers))
                    continue;

                row.CustomerTrxId ??= identifiers.CustomerTrxId;
                row.PaymentScheduleId ??= identifiers.PaymentScheduleId;
                if (row.InvoiceAmount <= 0)
                    row.InvoiceAmount = identifiers.InvoiceAmount;
                row.GlAccountingDate ??= identifiers.GlAccountingDate;
            }

            return rows;
        }

		private async Task<List<AdjustmentPostingDto>> AttachReceiptApplicationsAsync(List<AdjustmentPostingDto> rows)
		{
			if (!rows.Any())
				return rows;

			var requestIds = rows
				.Select(row => StagingHeaderIdHelper.Parse(row.HeaderId).RequestId)
				.Distinct()
				.ToList();

			var applicationRows = await _context.ReceiptInvoiceApplications
				.AsNoTracking()
				.Where(a => requestIds.Contains(a.RequestId) && a.IsActive)
				.Select(a => new
				{
					a.RequestId,
					a.ReceiptId,
					a.InvoiceId,
					a.InvoiceNumber,
					a.PaymentScheduleId,
					a.AppliedAmount,
					a.DiscountAmount,
					a.AmountDue,
					a.CurrencyCode
				})
				.ToListAsync();

			var grouped = applicationRows
				.GroupBy(a => a.RequestId)
				.ToDictionary(
					g => g.Key,
					g => g.Select(a => new ReceiptInvoiceApplicationPostingDto
					{
						InvoiceId = a.InvoiceId,
						InvoiceNumber = a.InvoiceNumber,
						PaymentScheduleId = a.PaymentScheduleId,
						AppliedAmount = a.AppliedAmount,
						DiscountAmount = a.DiscountAmount,
						AmountDue = a.AmountDue,
						CurrencyCode = a.CurrencyCode
					}).ToList());

			// The persisted request flag is authoritative. If Apply Receipt was requested but
			// no application rows exist, the row is still flagged (with an empty list) so the
			// Oracle API rejects the posting instead of posting only the write-off.
			var applyReceiptRequestIds = (await _context.Requests
				.AsNoTracking()
				.Where(r => requestIds.Contains(r.Id) && r.ApplyReceiptEnabled)
				.Select(r => r.Id)
				.ToListAsync())
				.ToHashSet();

			foreach (var row in rows)
			{
				var requestId = StagingHeaderIdHelper.Parse(row.HeaderId).RequestId;
				if (applyReceiptRequestIds.Contains(requestId))
					row.ApplyReceiptEnabled = true;

				if (!grouped.TryGetValue(requestId, out var applications) || applications.Count == 0)
					continue;

				row.ApplyReceiptEnabled = true;
				row.ReceiptApplications = applications;
				row.ReceiptId ??= applicationRows
					.Where(a => a.RequestId == requestId)
					.Select(a => a.ReceiptId)
					.FirstOrDefault();
			}

			return rows;
		}
    }
}
