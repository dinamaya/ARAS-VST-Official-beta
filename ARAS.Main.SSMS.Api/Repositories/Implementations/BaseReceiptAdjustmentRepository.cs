using ARAS.Main.SSMS.Api.Context;
using ARAS.Main.SSMS.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.Repositories.Interfaces;
using ARAS.Main.SSMS.Api.Services.Interfaces;

namespace ARAS.Main.SSMS.Api.Repositories.Implementations
{
    public class BaseReceiptAdjustmentRepository<TCreate> : IBaseReceiptAdjustmentRepository<TCreate>
		where TCreate : BaseAdjustmentCreateDto
	{
		private readonly MainDbContext _context;
		private readonly IBackgroundJobService _bgJobService;
		private readonly IAdjustmentRepository _adjustmentRepo;
		private readonly IRequestRepository _requestRepo;
		private readonly ITransactionRepository _transactionRepo;
		private readonly IRemarksRepository _remarksRepo;
		private readonly IInvoiceRepository _invoiceRepo;

        public BaseReceiptAdjustmentRepository(
            MainDbContext context, 
            IBackgroundJobService bgJobService, 
            IAdjustmentRepository adjustmentRepo, 
            IRequestRepository requestRepo, 
            ITransactionRepository transactionRepo, 
            IRemarksRepository remarksRepo, 
            IInvoiceRepository invoiceRepo)
        {
            _context = context;
            _bgJobService = bgJobService;
            _adjustmentRepo = adjustmentRepo;
            _requestRepo = requestRepo;
            _transactionRepo = transactionRepo;
            _remarksRepo = remarksRepo;
            _invoiceRepo = invoiceRepo;
        }

		public async Task<long> Create(RequestCreationDto<TCreate> data, string createdBy, string adjustmentTypeCode)
		{
			await using var dbTransaction = await _context.Database.BeginTransactionAsync();

			try
			{
				var adjustmentType = await _adjustmentRepo.GetAdjustmentInfoByCode(adjustmentTypeCode);
				var requestId = await _requestRepo.CreateAsync(adjustmentType.Id, createdBy);

				var transaction = new TransactionCreateDto(requestId, "Pending");
				var transactId = await _transactionRepo.CreateAsync(transaction, createdBy);

				var invoice = new InvoiceCreateDto(data.Model);
				var invoiceId = await _invoiceRepo.CreateAsync(invoice, createdBy);

				var adjustment = new AdjustmentCreateDto(invoiceId, requestId, data.Model, adjustmentType.Id);
				await _adjustmentRepo.CreateAsync(adjustment, createdBy);

				await dbTransaction.CommitAsync();

				return requestId;
			}
			catch
			{
				await dbTransaction.RollbackAsync();
				throw;
			}
		}

		public async Task<long> Update(long requestId, RequestCreationDto<TCreate> data, string modifiedBy, string adjustmentTypeCode)
		{
			await using var dbTransaction = await _context.Database.BeginTransactionAsync();

			try
			{
				var requestRefNo = await _requestRepo.GetRequestNumberById(requestId);
				var adjustmentType = await _adjustmentRepo.GetAdjustmentInfoByCode(adjustmentTypeCode);

				// Implement Update here no need to deactivate because it is only a single data
				await dbTransaction.CommitAsync();
				return requestId;
			}
			catch
			{
				await dbTransaction.RollbackAsync();
				throw;
			}
		}
	}
}
