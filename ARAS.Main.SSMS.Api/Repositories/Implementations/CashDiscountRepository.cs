using ARAS.Main.Oracle.Api.Models.Dtos;
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
	public class CashDiscountRepository : ICashDiscountRepository
	{
		private readonly MainDbContext _context;
		private readonly IStatusRepository _statusRepo;
		private readonly IRequestRepository _requestRepo;
		private readonly ITransactionRepository _transactionRepo;
		private readonly IRemarksRepository _remarksRepo;
		private readonly IAdjustmentRepository _adjustmentRepo;
		private readonly IInvoiceRepository _invoiceRepo;
		private readonly IBackgroundJobService _bgJobService;

		public CashDiscountRepository(MainDbContext context, IStatusRepository statusRepo, IRequestRepository requestRepo, IAdjustmentRepository adjustmentRepo, IInvoiceRepository invoiceRepo, ITransactionRepository transactionRepo, IRemarksRepository remarksRepo, IBackgroundJobService bgJobService)
		{
			_context = context;
			_statusRepo = statusRepo;
			_requestRepo = requestRepo;
			_adjustmentRepo = adjustmentRepo;
			_invoiceRepo = invoiceRepo;
			_transactionRepo = transactionRepo;
			_remarksRepo = remarksRepo;
			_bgJobService = bgJobService;
		}

		public async Task<long> CreateAsync(RequestCreationDto<AdjustmentRequestCreationDto<CashDiscountCreateDto>> data, string createdBy)
		{
			ArgumentNullException.ThrowIfNull(data, nameof(CashDiscountCreateDto));

			if (string.IsNullOrEmpty(data.GroupCode))
				throw new InvalidOperationException(Exceptions.EMPTY_GROUP_CODE);

			if (!data.Model.Adjustments.Any())
				throw new ArgumentNullException(Exceptions.EMPTY_CASHDISCOUNT_ROWS);

			await using var dbTransaction = await _context.Database.BeginTransactionAsync();

			try
			{
				string cashDiscountTypeId = await _context.AdjustmentTypes.Where(x => x.Code.Equals("CDR")).Select(x => x.Id).FirstAsync();
				string referenceNo = await _adjustmentRepo.GenerateReferenceNumber(data.GroupCode, "CDR");

				var request = new RequestCreateDto(referenceNo, cashDiscountTypeId);
				var requestId = await _requestRepo.CreateAsync(request, createdBy);

				var transaction = new TransactionCreateDto(requestId, "Pending");
				var transactId = await _transactionRepo.CreateAsync(transaction, createdBy);

				foreach (var item in data.Model.Adjustments)
					await CreateInvoiceAdjustments(createdBy, requestId, cashDiscountTypeId, item);

				var timeline = await _transactionRepo.GetEmailHistoryByRequestId(requestId);

				await _bgJobService.RunSendRequestPending(new RequestPendingDto
				{
					RequestId = requestId.ToString(),
					RequestorName = data.CreatorFullName,
					AdjustmentType = "Cash Discount",
					RequestNumber = referenceNo,
					Status = "Pending",
					Timeline = timeline,
					ToEmail = data.Model.ToEmail,
				});

				await dbTransaction.CommitAsync();

				return requestId;
			}
			catch
			{
				await dbTransaction.RollbackAsync();
				throw;
			}
		}

		public async Task UpdateAsync(long requestId, RequestCreationDto<AdjustmentRequestCreationDto<CashDiscountCreateDto>> data, string modifiedBy)
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
				ArgumentNullException.ThrowIfNull(data, nameof(CashDiscountCreateDto));
				Guards.ThrowInvalidOperationIf(!data.Model.Adjustments.Any(), Exceptions.EMPTY_CASHDISCOUNT_ROWS);

				var requestRefNo = await _requestRepo.GetRequestNumberById(requestId);
				string cashDiscountTypeId = await _context.AdjustmentTypes.Where(x => x.Code.Equals("CDR")).Select(x => x.Id).FirstAsync();

				var transaction = new TransactionCreateDto(requestId, "Pending");
				var transactId = await _transactionRepo.CreateAsync(transaction, modifiedBy);

				await _adjustmentRepo.DeactivateAllByRequestId(requestId);

				foreach (var item in data.Model.Adjustments)
					await CreateInvoiceAdjustments(modifiedBy, requestId, cashDiscountTypeId, item);


				var timeline = await _transactionRepo.GetEmailHistoryByRequestId(requestId);

				await _bgJobService.RunSendRequestPending(new RequestPendingDto
				{
					RequestId = requestId.ToString(),
					RequestorName = data.CreatorFullName,
					AdjustmentType = "Cash Discount",
					RequestNumber = requestRefNo,
					Status = "Approved",
					Timeline = timeline,
					ToEmail = data.Model.ToEmail,
				});

				await dbTransaction.CommitAsync();
			}
			catch
			{
				await dbTransaction.RollbackAsync();
				throw;
			}
		}

		public async Task CreateApproveTransaction(RequestUpdateDto data, string createdBy)
		{
			await using var dbTransaction = await _context.Database.BeginTransactionAsync();

			try
			{
				bool isApprovable = await _requestRepo.IsApprovable(data.RequestId);
				Guards.ThrowInvalidOperationIf(!isApprovable, Exceptions.ALREADY_APPROVED);

				var transaction = new TransactionCreateDto(data.RequestId, "Approved");
				var transactId = await _transactionRepo.CreateAsync(transaction, createdBy);

				var timeline = await _transactionRepo.GetEmailHistoryByRequestId(data.RequestId);
				var request = await _requestRepo.GetForEmailDetailsById(data.RequestId);

				await _bgJobService.RunSendRequestApproved(new RequestPendingDto
				{
					RequestId = data.RequestId.ToString(),
					RequestorName = request.Creator,
					AdjustmentType = "Cash Discount",
					RequestNumber = request.RequestNumber,
					Status = "Approved",
					Timeline = timeline,
					ToEmail = data.ToEmail,
				});

				await dbTransaction.CommitAsync();
			}
			catch
			{
				await dbTransaction.RollbackAsync();
				throw;
			}
		}

		public async Task CreateValidateTransaction(long requestId, string createdBy)
		{
			await using var dbTransaction = await _context.Database.BeginTransactionAsync();

			try
			{
				bool isValidatable = await _requestRepo.IsValidatable(requestId);
				Guards.ThrowInvalidOperationIf(!isValidatable, Exceptions.ALREADY_VALIDATED);

				var transaction = new TransactionCreateDto(requestId, "Validated");
				var transactId = await _transactionRepo.CreateAsync(transaction, createdBy);

				await dbTransaction.CommitAsync();
			}
			catch
			{
				await dbTransaction.RollbackAsync();
				throw;
			}
		}

		public async Task CreateDeclineTransaction(CreateDeclineDto createDecline, string createdBy)
		{
			await using var dbTransaction = await _context.Database.BeginTransactionAsync();

			try
			{
				bool isDeclinable = await _requestRepo.IsDeclinable(createDecline.RequestId);
				Guards.ThrowInvalidOperationIf(!isDeclinable, Exceptions.ALREADY_DECLINED);

				var transaction = new TransactionCreateDto(createDecline.RequestId, "Declined");
				var transactId = await _transactionRepo.CreateAsync(transaction, createdBy);

				var remarks = new RemarksCreateDto(transactId, createDecline.Remarks);
				await _remarksRepo.CreateAsync(remarks, createdBy);

				await dbTransaction.CommitAsync();
			}
			catch
			{
				await dbTransaction.RollbackAsync();
				throw;
			}
		}

		public async Task CreateRejectTransaction(long requestId, string createdBy)
		{
			await using var dbTransaction = await _context.Database.BeginTransactionAsync();

			try
			{
				bool isRejectable = await _requestRepo.IsRejectable(requestId);
				Guards.ThrowInvalidOperationIf(!isRejectable, Exceptions.NOT_REJECTABLE);

				var transaction = new TransactionCreateDto(requestId, "Rejected");
				var transactId = await _transactionRepo.CreateAsync(transaction, createdBy);

				await dbTransaction.CommitAsync();
			}
			catch
			{
				await dbTransaction.RollbackAsync();
				throw;
			}
		}

		public async Task<IEnumerable<TransactionRequestRowDto>> GetAllSubmissions()
		{
			return await _context.VwLatestRequestTransactions
				.OrderByDescending(t => t.TransactionId)
				.Select(t => new TransactionRequestRowDto()
				{
					RequestId = t.RequestId,
					RequestNumber = t.RequestNumber,

					Requestor = ValidateFullName(t.RequestorFirstName, t.RequestorLastName),
					DateRequested = t.DateRequested.ToString(Formats.Date.DISPLAY_COMPLETE),

					Approver = ValidateFullName(t.ApproverFirstName, t.ApproverLastName),
					DateApproved = t.DateApproved.HasValue ? ((DateTime)t.DateApproved).ToString(Formats.Date.DISPLAY_COMPLETE) : string.Empty,

					Validator = ValidateFullName(t.ValidatorFirstName, t.ValidatorLastName),
					DateValidated = t.DateValidated.HasValue ? ((DateTime)t.DateValidated).ToString(Formats.Date.DISPLAY_COMPLETE) : string.Empty,

					Creator = ValidateFullName(t.CreatorFirstName, t.CreatorLastName),
					DateCreated = t.DateCreated.ToString(Formats.Date.DISPLAY_COMPLETE),

					Status = t.Status,
				})
				.ToListAsync();
		}

		public async Task<IEnumerable<TransactionRequestRowDto>> GetAllForApprovals()
		{
			return await _context.VwLatestRequestTransactions
				.Where(t =>
					t.Status == "Pending" &&
					t.ApproverId == null && t.ValidatorId == null
				)
				.OrderByDescending(t => t.TransactionId)
				.Select(t => new TransactionRequestRowDto()
				{
					RequestId = t.RequestId,
					RequestNumber = t.RequestNumber,
					Requestor = ValidateFullName(t.RequestorFirstName, t.RequestorLastName),
					DateRequested = t.DateRequested.ToString(Formats.Date.DISPLAY_COMPLETE),

					Approver = ValidateFullName(t.ApproverFirstName, t.ApproverLastName),
					DateApproved = t.DateApproved.HasValue ? ((DateTime)t.DateApproved).ToString(Formats.Date.DISPLAY_COMPLETE) : string.Empty,

					Validator = ValidateFullName(t.ValidatorFirstName, t.ValidatorLastName),
					DateValidated = t.DateValidated.HasValue ? ((DateTime)t.DateValidated).ToString(Formats.Date.DISPLAY_COMPLETE) : string.Empty,

					Creator = ValidateFullName(t.CreatorFirstName, t.CreatorLastName),
					DateCreated = t.DateCreated.ToString(Formats.Date.DISPLAY_COMPLETE),

					Status = t.Status,
				})
				.ToListAsync();
		}

		public async Task<IEnumerable<TransactionRequestRowDto>> GetAllForValidations()
		{
			return await _context.VwLatestRequestTransactions
				.Where(t =>
					(t.Status == "Approved" || t.Status == "Pending") &&
					t.ApproverId != null && t.ValidatorId == null)
				.OrderByDescending(t => t.TransactionId)
				.Select(t => new TransactionRequestRowDto()
				{
					RequestId = t.RequestId,
					RequestNumber = t.RequestNumber,
					Requestor = ValidateFullName(t.RequestorFirstName, t.RequestorLastName),
					DateRequested = t.DateRequested.ToString(Formats.Date.DISPLAY_COMPLETE),

					Approver = ValidateFullName(t.ApproverFirstName, t.ApproverLastName),
					DateApproved = t.DateApproved.HasValue ? ((DateTime)t.DateApproved).ToString(Formats.Date.DISPLAY_COMPLETE) : string.Empty,

					Validator = ValidateFullName(t.ValidatorFirstName, t.ValidatorLastName),
					DateValidated = t.DateValidated.HasValue ? ((DateTime)t.DateValidated).ToString(Formats.Date.DISPLAY_COMPLETE) : string.Empty,

					Creator = ValidateFullName(t.CreatorFirstName, t.CreatorLastName),
					DateCreated = t.DateCreated.ToString(Formats.Date.DISPLAY_COMPLETE),

					Status = t.Status,
				})
				.ToListAsync();
		}

		public async Task<IEnumerable<CashDiscountRowDto>> GetAdjustmentsByRequestId(long requestId)
		{
			return await _context.VwCashDiscountAdjustments
				.Where(r => r.AdjustmentActivity == "Cash Discount" && r.RequestId == requestId)
				.Select(r => new CashDiscountRowDto()
				{
					Id = r.AdjustmentId.ToString(),
					DiscountValue = r.DiscountPercentage / 100,
					AdjustmentAmount = r.AdjustmentAmount,
					AdjustmentActivity = r.AdjustmentActivity,
					InvoiceAmount = r.InvoiceAmount,
					InvoiceDate = r.InvoiceDate.ToString(Formats.Date.DISPLAY),
					InvoiceNumber = r.InvoiceNumber,
					CustomerName = r.CustomerName,
					CustomerNumber = r.CustomerNumber,
					ReasonCode = "Discount",
					Remarks = r.Remarks,
				})
				.ToListAsync();
		}

		public async Task<TransactionRequestRowDto> GetTransactionRequestByRequestId(long requestId)
		{
			return await _context.VwLatestRequestTransactions.Where(t => t.RequestId == requestId)
				.Select(t => new TransactionRequestRowDto()
				{
					RequestId = t.RequestId,
					RequestNumber = t.RequestNumber,
					Requestor = ValidateFullName(t.RequestorFirstName, t.RequestorLastName),
					DateRequested = t.DateRequested.ToString(Formats.Date.DISPLAY_COMPLETE),

					Approver = ValidateFullName(t.ApproverFirstName, t.ApproverLastName),
					DateApproved = t.DateApproved.HasValue ? ((DateTime)t.DateApproved).ToString(Formats.Date.DISPLAY_COMPLETE) : string.Empty,

					Validator = ValidateFullName(t.ValidatorFirstName, t.ValidatorLastName),
					DateValidated = t.DateValidated.HasValue ? ((DateTime)t.DateValidated).ToString(Formats.Date.DISPLAY_COMPLETE) : string.Empty,

					Creator = ValidateFullName(t.CreatorFirstName, t.CreatorLastName),
					DateCreated = t.DateCreated.ToString(Formats.Date.DISPLAY_COMPLETE),

					Status = t.Status,
				})
				.FirstOrDefaultAsync();
		}

		private static string ValidateFullName(string fName, string lName) =>
			string.IsNullOrEmpty(lName) && string.IsNullOrEmpty(fName) ? string.Empty : lName + ", " + fName;

		private async Task<Transaction> GetLatestTransactionByRequestId(long requestId)
		{
			return await _context.Transactions
				.Where(t => t.RequestId == requestId)
				.OrderByDescending(t => t.Id)
				.FirstOrDefaultAsync() ?? throw new InvalidOperationException(Exceptions.NOTFOUND_TRANSACTION);
		}

		private async Task CreateInvoiceAdjustments(string createdBy, long requestId, string adjustmentTypeId, CashDiscountCreateDto data)
		{
			var invoice = new InvoiceCreateDto(data.InvoiceNumber, data.InvoiceAmount, data.InvoiceDate, data.CustomerNumber, data.CustomerName);
			var invoiceId = await _invoiceRepo.CreateAsync(invoice, createdBy);

			double adjustmentAmount = data.DiscountValue * data.InvoiceAmount;
			float discountPercent = data.DiscountValue * 100;
			var adjustment = new AdjustmentCreateDto(invoiceId, requestId, adjustmentAmount, adjustmentTypeId, discountPercent, data.Remarks);
			await _adjustmentRepo.CreateAsync(adjustment, createdBy);
		}

		public async Task<bool> IsValid(CashDiscountCreateValidationDto cashCreateValidationRequest)
		{
			return await _invoiceRepo.IsInvoiceNumberAvailable(cashCreateValidationRequest.InvoiceNumber, "CDR");
		}
	}
}
