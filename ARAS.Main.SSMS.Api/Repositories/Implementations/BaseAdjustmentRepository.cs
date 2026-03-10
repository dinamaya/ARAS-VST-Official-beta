using ARAS.Main.SSMS.Api.App_Code.Globals;
using ARAS.Main.SSMS.Api.App_Code.Globals.Constants;
using ARAS.Main.SSMS.Api.Context;
using ARAS.Main.SSMS.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.Models.Entities;
using ARAS.Main.SSMS.Api.Repositories.Interfaces;
using ARAS.Main.SSMS.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ARAS.Main.SSMS.Api.Repositories.Implementations
{
	public class BaseAdjustmentRepository<TCreate> : IBaseAdjustmentRepository<TCreate>
		where TCreate : BaseAdjustmentCreateDto
	{
		private readonly MainDbContext _context;
		private readonly IBackgroundJobService _bgJobService;
		private readonly IAdjustmentRepository _adjustmentRepo;
		private readonly IRequestRepository _requestRepo;
		private readonly ITransactionRepository _transactionRepo;
		private readonly IRemarksRepository _remarksRepo;
		private readonly IInvoiceRepository _invoiceRepo;

        public BaseAdjustmentRepository(
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

        public async Task<long> Create(
			RequestCreationDto<TCreate> data, 
			string createdBy, 
			string adjustmentTypeCode,
			Func<TCreate, string, long, Task> createInvoiceCallBack)
		{
			//Guards.ThrowInvalidOperationIf(!data.Model.Any(), Exceptions.EMPTY_CASHDISCOUNT_ROWS);

			await using var dbTransaction = await _context.Database.BeginTransactionAsync();

			try
			{
				var adjustmentType = await _adjustmentRepo.GetAdjustmentInfoByCode(adjustmentTypeCode);
				var requestId = await _requestRepo.CreateAsync(adjustmentType.Id, createdBy);

				var transaction = new TransactionCreateDto(requestId, "For CNC Approval");
				var transactId = await _transactionRepo.CreateAsync(transaction, createdBy);

				var invoice = new InvoiceCreateDto(data.Model);
				var invoiceId = await _invoiceRepo.CreateAsync(invoice, createdBy);

				//var adjustment = new AdjustmentCreateDto(invoiceId, requestId, data.Model, adjustmentType.Id);
				//await _adjustmentRepo.CreateAsync(adjustment, createdBy);

				await dbTransaction.CommitAsync();

				return requestId;
			}
			catch
			{
				await dbTransaction.RollbackAsync();
				throw;
			}
		}

		public async Task<long> Update(
			long requestId,
			RequestCreationDto<TCreate> data,
			string modifiedBy,
			string adjustmentTypeCode,
			string adjustmentRouteName,
			Func<TCreate, string, long, Task> updateInvoiceCallBack)
		{
			// Fetch the existing transaction by request Id
			// Create new Transaction with the status Pending
			// -- set the other details to the existing transaction
			// Fetch all Existing Adjustments and Invoice by RequestId
			// Deactivate the existing or previous adjustments and invoices
			// Insert both the updated and new adjustments and invoices

			await using var dbTransaction = await _context.Database.BeginTransactionAsync();

			try
			{
				var requestRefNo = await _requestRepo.GetRequestNumberById(requestId);
				var adjustmentType = await _adjustmentRepo.GetAdjustmentInfoByCode(adjustmentTypeCode);

				var prevTimeline = await _transactionRepo.GetHistoryByRequestId(requestId);
				var prevCreatorRole = prevTimeline.LastOrDefault().AccountRole;

				var transaction = new TransactionCreateDto(requestId, "For CNC Approval");
				var transactId = await _transactionRepo.CreateAsync(transaction, modifiedBy);

				if(adjustmentTypeCode.Equals("arr"))
					await _adjustmentRepo.DeactivateAPARByRequestId(requestId);
				else
					await _adjustmentRepo.DeactivateAllByRequestId(requestId);

				await updateInvoiceCallBack.Invoke(data.Model, adjustmentType.Id, requestId);

				await dbTransaction.CommitAsync();
				return requestId;
			}
			catch
			{
				await dbTransaction.RollbackAsync();
				throw;
			}
		}

		public async Task Approve(RequestUpdateDto data, string createdBy, string adjustmentTypeCode)
		{
			await using var dbTransaction = await _context.Database.BeginTransactionAsync();

			try
			{
				bool isApprovable = await _requestRepo.IsApprovable(data.RequestId);
				var adjustmentType = await _adjustmentRepo.GetAdjustmentInfoByCode(adjustmentTypeCode);
				Guards.ThrowInvalidOperationIf(!isApprovable, Exceptions.ALREADY_APPROVED);

				var transaction = new TransactionCreateDto(data.RequestId, "For ERP Posting");
				var transactId = await _transactionRepo.CreateAsync(transaction, createdBy);

				var timeline = await _transactionRepo.GetEmailHistoryByRequestId(data.RequestId);
				var request = await _requestRepo.GetForEmailDetailsById(data.RequestId);

				await dbTransaction.CommitAsync();
			}
			catch
			{
				await dbTransaction.RollbackAsync();
				throw;
			}
		}

		public async Task Decline(NegateRequestDto data, string createdBy, string adjustmentTypeCode)
		{
			await using var dbTransaction = await _context.Database.BeginTransactionAsync();

			try
			{
				var adjustmentType = await _adjustmentRepo.GetAdjustmentInfoByCode(adjustmentTypeCode);
				bool isDeclinable = await _requestRepo.IsDeclinable(data.RequestId);
				Guards.ThrowInvalidOperationIf(!isDeclinable, Exceptions.ALREADY_DECLINED);

				var transaction = new TransactionCreateDto(data.RequestId, "Declined");
				var transactId = await _transactionRepo.CreateAsync(transaction, createdBy);

				var remarks = new RemarksCreateDto(transactId, data.Remarks);
				await _remarksRepo.CreateAsync(remarks, createdBy);

				var timeline = await _transactionRepo.GetEmailHistoryByRequestId(data.RequestId);
				var request = await _requestRepo.GetForEmailDetailsById(data.RequestId);
				var latestUpdateDetails = timeline.LastOrDefault();

				await dbTransaction.CommitAsync();
			}
			catch
			{
				await dbTransaction.RollbackAsync();
				throw;
			}
		}

		public async Task Reject(NegateRequestDto data, string createdBy, string adjustmentTypeCode)
		{
			await using var dbTransaction = await _context.Database.BeginTransactionAsync();

			try
			{
				var adjustmentType = await _adjustmentRepo.GetAdjustmentInfoByCode(adjustmentTypeCode);
				bool isRejectable = await _requestRepo.IsRejectable(data.RequestId);
				Guards.ThrowInvalidOperationIf(!isRejectable, Exceptions.NOT_REJECTABLE);

				var transaction = new TransactionCreateDto(data.RequestId, "Rejected");
				var transactId = await _transactionRepo.CreateAsync(transaction, createdBy);

				var timeline = await _transactionRepo.GetEmailHistoryByRequestId(data.RequestId);
				var request = await _requestRepo.GetForEmailDetailsById(data.RequestId);
				var latestUpdateDetails = timeline.LastOrDefault();

				await dbTransaction.CommitAsync();
			}
			catch
			{
				await dbTransaction.RollbackAsync();
				throw;
			}
		}
	}
}
