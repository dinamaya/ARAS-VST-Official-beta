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

				Guards.ThrowInvalidOperationIf(!string.IsNullOrEmpty(existingAdjustments), "Invoice Number already has the following adjustments:\n" + existingAdjustments);

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
					.Where(z => z.model.ApplyReceiptEnabled && z.model.ReceiptApplications is not null && z.model.ReceiptApplications.Any())
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
						}));

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
			
			return new ReceiptAdjustmentUpdateResponseDto
			{
				AdjustmentAmount = adjustment.AdjustmentAmount,
				Remarks = adjustment.Remarks,
				Invoice = invoice,
				ApplyReceiptEnabled = receiptApplications.Count > 0,
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

		public async Task<IEnumerable<AdjustmentPostingDto>> GetStagingDataByAdjustmentId(long adjustmentId)
        {
            var rows = await _context.VwStagingRequestAdjustment
				.Where(a => a.AdjustmentId == adjustmentId)
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

		public async Task<IEnumerable<AdjustmentPostingDto>> GetStagingDataByAdjustmentId(IEnumerable<long> adjustmentIds)
		{
			adjustmentIds = [.. adjustmentIds.Distinct()];
            if (!adjustmentIds.Any()) return [];

            var keyPairs = adjustmentIds
                .Select(StagingHeaderIdHelper.Parse)
                .ToList();

            var requestIds = keyPairs
                .Select(x => x.RequestId)
                .Distinct()
                .ToList();

            var keySet = keyPairs
                .Select(x => $"{x.RequestId}:{x.AdjustmentId}")
                .ToHashSet();

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
                })
                .ToListAsync();

            rows = rows.Where(x =>
            {
                var parsed = StagingHeaderIdHelper.Parse(x.HeaderId);
                return keySet.Contains($"{parsed.RequestId}:{parsed.AdjustmentId}");
            }).ToList();

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

			foreach (var row in rows)
			{
				var requestId = StagingHeaderIdHelper.Parse(row.HeaderId).RequestId;
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
