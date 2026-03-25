using ARAS.Main.SSMS.Api.App_Code.Globals;
using ARAS.Main.SSMS.Api.App_Code.Globals.Helpers;
using ARAS.Main.SSMS.Api.Context;
using ARAS.Main.SSMS.Api.Models.Dtos;
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
				string invoiceNumber = data.Model.Select(d => d.InvoiceNumber).First();
				IList<string> adjustmentTypeIds = [];
				IEnumerable<long> requestIds = [];

				IList<TransactionCreateDto> transactions = [];
				IEnumerable<long> transactionIds = [];

				IList<InvoiceCreateDto> invoices = [];
				IEnumerable<long> invoiceIds = [];

				IList<AdjustmentCreateDto> adjustments = [];

				foreach (string adjustmentName in data.Model.Select(d => d.AdjustmentType))
				{
					string adjustmentType = (await _adjustmentRepo.GetAdjustmentInfoByName(adjustmentName)).Id;
					adjustmentTypeIds.Add(adjustmentType);
				}

				requestIds = await _requestRepo.CreateAsync(adjustmentTypeIds, createdBy);
				
				string existingAdjustments = await _requestRepo.InvoiceExistingAdjustments(invoiceNumber, adjustmentTypeIds);

				Guards.ThrowInvalidOperationIf(!string.IsNullOrEmpty(existingAdjustments), "Invoice Number already has the following adjustments:\n" + existingAdjustments);

				foreach (var requestId in requestIds)
					transactions.Add(new TransactionCreateDto(requestId, "For CNC Approval"));
				transactionIds = await _transactionRepo.CreateAsync(transactions, createdBy);

				foreach (var invoice in data.Model)
					invoices.Add(new InvoiceCreateDto(invoice));
				invoiceIds = await _invoiceRepo.CreateAsync(invoices, createdBy);

				// Combine the model, invoiceIds and requestIds
				var zipped = invoiceIds
					.Zip(data.Model, (inv, mod) => new
					{
						invoice = inv,
						model = mod
					})
					.Zip(requestIds, (mod, req) => new
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
				await dbTransaction.CommitAsync();

				var zipResults = requestIds.Zip(data.Model, 
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
					a.AdjustmentAmount,
					a.Remarks,
				}).FirstOrDefaultAsync()
				?? throw new InvalidOperationException("Failed to fetch the active adjustment details.");
			var invoice = await _context.VwReceiptAdjustments.Where(a => a.RequestId == requestId)
				.Select(a => new InvoiceDetailsRequestDto
				{
					InvoiceNumber = a.InvoiceNumber,
					InvoiceAmount = a.InvoiceAmount,
					InvoiceDate = DateOnly.FromDateTime(a.InvoiceDate),
					CustomerName = a.CustomerName,
					CustomerNumber = a.CustomerNumber
				}).FirstOrDefaultAsync();

			var transactionHistories = await _transactionRepo.GetHistoryByRequestId(requestId);
			var notes = await _notesService.GetByRequestId(requestId);
			
			return new ReceiptAdjustmentUpdateResponseDto
			{
				AdjustmentAmount = adjustment.AdjustmentAmount,
				Remarks = adjustment.Remarks,
				Invoice = invoice,
				TransactionHistories = transactionHistories,
				Notes = notes
			};
		}

        public async Task<IEnumerable<AdjustmentPostingDto>> GetStagingDataByRequestId(long requestId)
        {
            return await _context.VwStagingRequestAdjustment
				.Where(a => a.RequestId == requestId)
				.Select(a => new AdjustmentPostingDto
				{
                    HeaderId = StagingHeaderIdHelper.Generate(a.RequestId, a.AdjustmentId),
                    InvoiceNumber = a.InvoiceNumber,
					AdjustmentAmount = a.AdjustmentAmount,
					InvoiceDate = a.InvoiceDate,
					PaymentScheduleId = null,
					DateApplied = DateTime.Now,
					AdjustmentActivity = a.Activity,
					ReasonCode = a.ReasonCode,
					Remarks = a.Remarks,
					CustomerName = a.CustomerName,
					CustomerNumber = a.CustomerNumber
				}).ToListAsync();
		}

		public async Task<IEnumerable<AdjustmentPostingDto>> GetStagingDataByRequestIds(IEnumerable<long> requestIds)
		{
			return await _context.VwStagingRequestAdjustment
			   .Where(a => requestIds.Contains(a.RequestId))
			   .Select(a => new AdjustmentPostingDto
			   {
                   HeaderId = StagingHeaderIdHelper.Generate(a.RequestId, a.AdjustmentId),
                   AdjustmentId = a.AdjustmentId,
                   InvoiceNumber = a.InvoiceNumber,
				   AdjustmentAmount = a.AdjustmentAmount,
				   InvoiceDate = a.InvoiceDate,
				   PaymentScheduleId = null,
				   DateApplied = DateTime.Now,
				   AdjustmentActivity = a.Activity,
				   ReasonCode = a.ReasonCode,
				   Remarks = a.Remarks,
				   CustomerName = a.CustomerName,
				   CustomerNumber = a.CustomerNumber
			   }).ToListAsync();
		}

		public async Task<IEnumerable<AdjustmentPostingDto>> GetStagingDataByAdjustmentId(long adjustmentId)
        {
			return await _context.VwStagingRequestAdjustment
				.Where(a => a.AdjustmentId == adjustmentId)
				.Select(a => new AdjustmentPostingDto
				{
                    HeaderId = StagingHeaderIdHelper.Generate(a.RequestId, a.AdjustmentId),
                    InvoiceNumber = a.InvoiceNumber,
					AdjustmentAmount = a.AdjustmentAmount,
					InvoiceDate = a.InvoiceDate,
					PaymentScheduleId = null,
					DateApplied = DateTime.Now,
					AdjustmentActivity = a.Activity,
					ReasonCode = a.ReasonCode,
					Remarks = a.Remarks,
					CustomerName = a.CustomerName,
					CustomerNumber = a.CustomerNumber
				}).ToListAsync();
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
					AdjustmentAmount = a.AdjustmentAmount,
					InvoiceDate = a.InvoiceDate,
					PaymentScheduleId = null,
					DateApplied = DateTime.Now,
					AdjustmentActivity = a.Activity,
					ReasonCode = a.ReasonCode,
					Remarks = a.Remarks,
					CustomerName = a.CustomerName,
					CustomerNumber = a.CustomerNumber
                })
                .ToListAsync();

            return rows.Where(x =>
            {
                var parsed = StagingHeaderIdHelper.Parse(x.HeaderId);
                return keySet.Contains($"{parsed.RequestId}:{parsed.AdjustmentId}");
            });
        }
    }
}
