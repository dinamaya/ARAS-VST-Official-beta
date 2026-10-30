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
				ValidateSourceRules(models);
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

		private static readonly HashSet<string> ApplyReceiptAdjustmentTypes =
			new(StringComparer.OrdinalIgnoreCase) { "Write-Off: Small Amount", "Bank Charges" };

		public const string ReceiptSource = "Receipt";
		public const string InvoiceSource = "Invoice";

		/// <summary>
		/// Server-side rules for which document type each receipt adjustment can be raised against.
		/// Mirrors the request page so the API cannot be used to bypass it:
		/// - Cash Discount: invoices only (posted as an invoice adjustment).
		/// - Bank Charges: receipts only (posted as a receipt activity).
		/// - Write-Off: Small Amount: receipts or invoices.
		/// - Apply Receipt: receipts only, with Write-Off: Small Amount or Bank Charges, once per submission.
		/// </summary>
		private static void ValidateSourceRules(IList<BaseReceiptAdjustmentCreateDto> models)
		{
			Guards.ThrowInvalidOperationIf(models.Count == 0, "No adjustments were submitted.");

			var sources = models
				.Select(m => string.IsNullOrWhiteSpace(m.SourceType) ? ReceiptSource : m.SourceType.Trim())
				.Distinct(StringComparer.OrdinalIgnoreCase)
				.ToList();

			Guards.ThrowInvalidOperationIf(
				sources.Count > 1,
				"All adjustments in one submission must be for the same receipt or invoice.");

			var source = sources[0];
			bool isReceipt = string.Equals(source, ReceiptSource, StringComparison.OrdinalIgnoreCase);
			bool isInvoice = string.Equals(source, InvoiceSource, StringComparison.OrdinalIgnoreCase);

			Guards.ThrowInvalidOperationIf(
				!isReceipt && !isInvoice,
				$"Unknown source type \"{source}\". Expected \"{ReceiptSource}\" or \"{InvoiceSource}\".");

			// Apply Receipt applies the receipt to invoices once; only one request in a
			// submission may carry it, and only Write-Off: Small Amount or Bank Charges.
			var applyReceiptModels = models
				.Where(m => m.ApplyReceiptEnabled || (m.ReceiptApplications?.Any() ?? false))
				.ToList();

			Guards.ThrowInvalidOperationIf(
				applyReceiptModels.Count > 1,
				"Apply Receipt can only be submitted with one adjustment per receipt.");

			Guards.ThrowInvalidOperationIf(
				applyReceiptModels.Any(m => !ApplyReceiptAdjustmentTypes.Contains(m.AdjustmentType)),
				"Apply Receipt can only be used with Write-Off: Small Amount or Bank Charges.");

			foreach (var model in models)
			{
				Guards.ThrowInvalidOperationIf(
					isReceipt && string.Equals(model.AdjustmentType, "Cash Discount", StringComparison.OrdinalIgnoreCase),
					"Cash Discount can only be applied to invoices. Please select an invoice.");

				Guards.ThrowInvalidOperationIf(
					isInvoice && string.Equals(model.AdjustmentType, "Bank Charges", StringComparison.OrdinalIgnoreCase),
					"Bank Charges can only be applied to receipts. Please select a receipt.");

				Guards.ThrowInvalidOperationIf(
					isInvoice && (model.ApplyReceiptEnabled || (model.ReceiptApplications?.Any() ?? false)),
					"Apply Receipt can only be used with receipts. Please select a receipt.");
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

				await UpdateReceiptApplicationsAsync(requestId, data, modifiedBy);

				await dbTransaction.CommitAsync();
				return requestId;
			}
			catch
			{
				await dbTransaction.RollbackAsync();
				throw;
			}
		}

		/// <summary>
		/// Applies the requestor's Apply Receipt corrections (when sent) and checks that the
		/// adjustment plus the invoice applications still fit within the receipt balance.
		/// Only the request that already carries Apply Receipt can change it, so the same
		/// invoices are never applied by two requests of one submission.
		/// </summary>
		private async Task UpdateReceiptApplicationsAsync(long requestId, ReceiptAdjustmentUpdateRequestDto data, string modifiedBy)
		{
			var request = await _context.Requests.FirstOrDefaultAsync(r => r.Id == requestId)
				?? throw new InvalidOperationException("Request not found. Please contact the administrator");

			Guards.ThrowInvalidOperationIf(
				string.Equals(request.SourceType, InvoiceSource, StringComparison.OrdinalIgnoreCase) &&
				(data.ApplyReceiptEnabled == true || (data.ReceiptApplications?.Any() ?? false)),
				"Apply Receipt can only be used with receipts.");

			var existing = await _context.ReceiptInvoiceApplications
				.Where(a => a.RequestId == requestId && a.IsActive)
				.ToListAsync();

			double appliedTotal;

			if (data.ApplyReceiptEnabled is null)
			{
				appliedTotal = existing.Sum(a => a.AppliedAmount);
			}
			else
			{
				Guards.ThrowInvalidOperationIf(
					!request.ApplyReceiptEnabled && existing.Count == 0,
					"Apply Receipt can only be changed on the request it was submitted with.");

				var applications = data.ApplyReceiptEnabled.Value
					? (data.ReceiptApplications ?? [])
						.Where(a => a is not null && a.PaymentScheduleId > 0 && a.AppliedAmount > 0)
						.ToList()
					: new List<ReceiptInvoiceApplicationSelectionDto>();

				Guards.ThrowInvalidOperationIf(
					data.ApplyReceiptEnabled.Value && applications.Count == 0,
					"Apply Receipt is enabled but no invoice with a positive applied amount was selected.");

				Guards.ThrowInvalidOperationIf(
					applications.GroupBy(a => a.PaymentScheduleId).Any(g => g.Count() > 1),
					"The same invoice was selected more than once. Please select each invoice only once.");

				foreach (var application in applications)
					Guards.ThrowInvalidOperationIf(
						application.AmountDue > 0 && application.AppliedAmount > application.AmountDue + 0.005,
						$"The applied amount for invoice {application.InvoiceNumber} is more than its amount due.");

				var receipt = existing.FirstOrDefault();
				Guards.ThrowInvalidOperationIf(
					applications.Count > 0 && (receipt?.ReceiptId is null || receipt.ReceiptId <= 0),
					"The receipt ID for Apply Receipt is missing. Please contact the administrator.");

				var now = DateTime.Now;
				foreach (var old in existing)
				{
					old.IsActive = false;
					old.ModifiedBy = modifiedBy;
					old.DateModified = now;
				}

				if (applications.Count > 0)
				{
					await _context.ReceiptInvoiceApplications.AddRangeAsync(applications.Select(a => new ReceiptInvoiceApplication
					{
						RequestId = requestId,
						ReceiptNumber = receipt!.ReceiptNumber,
						ReceiptId = receipt.ReceiptId,
						InvoiceId = a.InvoiceId,
						InvoiceNumber = a.InvoiceNumber,
						PaymentScheduleId = a.PaymentScheduleId,
						AppliedAmount = a.AppliedAmount,
						DiscountAmount = a.DiscountAmount,
						AmountDue = a.AmountDue,
						CurrencyCode = a.CurrencyCode,
						CreatedBy = modifiedBy,
						DateCreated = now,
						ModifiedBy = modifiedBy,
						DateModified = now,
						IsActive = true
					}));
				}

				request.ApplyReceiptEnabled = applications.Count > 0;
				appliedTotal = applications.Sum(a => a.AppliedAmount);
			}

			// The receipt balance saved with the request (receipts use the legacy Invoice* columns).
			var receiptBalance = await _context.Adjustments
				.Where(a => a.RequestId == requestId && a.IsActive)
				.Select(a => a.Invoice.InvoiceBalance)
				.FirstOrDefaultAsync();

			if (receiptBalance > 0)
			{
				var total = Math.Round(appliedTotal + data.AdjustmentAmount, 2);
				Guards.ThrowInvalidOperationIf(
					total > Math.Round(receiptBalance, 2) + 0.005,
					$"The adjustment amount plus the applied invoice amounts ({total:N2}) is more than the receipt balance ({receiptBalance:N2}).");
			}

			await _context.SaveChangesAsync();
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
			// Also pass the request's source document type ("Receipt"/"Invoice") so the
			// Oracle API posts to the right Fusion service instead of guessing.
			var requestInfo = await _context.Requests
				.AsNoTracking()
				.Where(r => requestIds.Contains(r.Id))
				.Select(r => new { r.Id, r.ApplyReceiptEnabled, r.SourceType })
				.ToDictionaryAsync(r => r.Id);

			foreach (var row in rows)
			{
				var requestId = StagingHeaderIdHelper.Parse(row.HeaderId).RequestId;
				if (requestInfo.TryGetValue(requestId, out var info))
				{
					if (info.ApplyReceiptEnabled)
						row.ApplyReceiptEnabled = true;

					row.SourceType ??= info.SourceType;
				}

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
