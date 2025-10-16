using ARAS.Main.Oracle.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.App_Code.Globals;
using ARAS.Main.SSMS.Api.App_Code.Globals.Constants;
using ARAS.Main.SSMS.Api.Context;
using ARAS.Main.SSMS.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.Models.Entities;
using ARAS.Main.SSMS.Api.Repositories.Interfaces;
using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using static ARAS.Main.SSMS.Api.App_Code.Globals.Constants.Formats;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ARAS.Main.SSMS.Api.Repositories.Implementations
{
	public class CashDiscountRepository : ICashDiscountRepository
	{
		private readonly MainDbContext _context;
		private readonly IStatusRepository _statusRepo;
		private readonly IRequestRepository _requestRepo;
		private readonly ITransactionRepository _transactionRepo;
		private readonly IAdjustmentRepository _adjustmentRepo;
		private readonly IInvoiceRepository _invoiceRepo;

		public CashDiscountRepository(MainDbContext context, IStatusRepository statusRepo, IRequestRepository requestRepo, IAdjustmentRepository adjustmentRepo, IInvoiceRepository invoiceRepo, ITransactionRepository transactionRepo)
		{
			_context = context;
			_statusRepo = statusRepo;
			_requestRepo = requestRepo;
			_adjustmentRepo = adjustmentRepo;
			_invoiceRepo = invoiceRepo;
			_transactionRepo = transactionRepo;
		}

		public async Task<long> CreateAsync(RequestCreationDto<CashDiscountCreateDto> data, string createdBy)
		{
			ArgumentNullException.ThrowIfNull(data, nameof(CashDiscountCreateDto));

			if (string.IsNullOrEmpty(data.GroupCode))
				throw new InvalidOperationException(Exceptions.EMPTY_GROUP_CODE);

			if (!data.Adjustments.Any())
				throw new ArgumentNullException(Exceptions.EMPTY_CASHDISCOUNT_ROWS);

			await using var dbTransaction = await _context.Database.BeginTransactionAsync();

			try
			{
				string cashDiscountTypeId = await _context.AdjustmentTypes.Where(x => x.Code.Equals("CDR")).Select(x => x.Id).FirstAsync();
				//string referenceNo = await _adjustmentRepo.GenerateReferenceNumber(data.GroupCode, "CDR");
				string referenceNo = $"SAMP-{Guid.NewGuid().ToString()}";

				var request = new RequestCreateDto(referenceNo, cashDiscountTypeId);
				var requestId = await _requestRepo.CreateAsync(request, createdBy);

				var transaction = new TransactionCreateDto(requestId, "Pending");
				var transactId = await _transactionRepo.CreateAsync(transaction, createdBy);

				foreach (var item in data.Adjustments)
				{
					var invoice = new InvoiceCreateDto(item.InvoiceNumber, item.InvoiceAmount, item.InvoiceDate, item.CustomerNumber, item.CustomerName);
					var invoiceId = await _invoiceRepo.CreateAsync(invoice, createdBy);

					double adjustmentAmount = item.DiscountValue * item.InvoiceAmount;
					float discountPercent = item.DiscountValue * 100;
					var adjustment = new AdjustmentCreateDto(invoiceId, requestId, adjustmentAmount, cashDiscountTypeId, discountPercent, item.Remarks);
					await _adjustmentRepo.CreateAsync(adjustment, createdBy);
				}

				await dbTransaction.CommitAsync();

				return requestId;
			}
			catch
			{
				await dbTransaction.RollbackAsync();
				throw;
			}
		}

		//private async Task TestCommits(long requestId, string createdBy)
		//{
		//	await CreateDeclineTransaction(requestId, "ACCdf886923c4044b3abfd2533531d3ae2a827b2debca6c40a28e46128abb85c506");
		//	await Resubmit(requestId, createdBy);
		//	await CreateApproveTransaction(requestId, "ACCdf886923c4044b3abfd2533531d3ae2a827b2debca6c40a28e46128abb85c506");
		//	await CreateValidateTransaction(requestId, "ACCe48a677d4f144d489981f5b1f4eb81da790a1c2109394979a637c227f0320f21");
		//}

		//private async Task Resubmit(long requestId, string createdBy)
		//{
		//	var transaction = new TransactionCreateDto(requestId, "Pending");
		//	var transactId = await _transactionRepo.CreateAsync(transaction, createdBy);
		//}

		public async Task UpdateAsync(long requestId, IEnumerable<CashDiscountCreateDto> data, string modifiedBy)
		{
			ArgumentNullException.ThrowIfNull(data, nameof(CashDiscountCreateDto));

			if (!data.Any())
				throw new ArgumentNullException(Exceptions.EMPTY_CASHDISCOUNT_ROWS);

			await using var dbTransaction = await _context.Database.BeginTransactionAsync();

			try
			{
				DateTime date = DateTime.Now;
				string cashDiscountTypeId = await _context.AdjustmentTypes.Where(x => x.Code.Equals("CDR")).Select(x => x.Id).FirstAsync();
				string pendingStatusId = await _context.Statuses.Where(x => x.Name.Equals("Pending")).Select(x => x.Id).FirstAsync();

				// Fetch the existing transaction by request Id
				// Create new Transaction with the status Pending
				// -- set the other details to the existing transaction

				// Fetch all Existing Adjustments and Invoice by RequestId
				// Deactivate the existing or previous adjustments and invoices
				// Insert both the updated and new adjustments and invoices

				var transactionRequest = await GetLatestTransactionByRequestId(requestId);

				var transaction = new Transaction();

				transaction.RequestId = requestId;

				transaction.CreatedBy = modifiedBy;
				transaction.DateCreated = DateTime.Now;

				transaction.StatusId = pendingStatusId;
				transaction.IsActive = true;

				await _context.Transactions.AddAsync(transaction);
				await _context.SaveChangesAsync();

				await _adjustmentRepo.DeactivateAllByRequestId(requestId);

				foreach (var item in data)
				{
					var invoice = new Invoice();
					invoice.InvoiceNumber = item.InvoiceNumber;
					invoice.InvoiceAmount = item.InvoiceAmount;
					invoice.InvoiceDate = item.InvoiceDate;
					invoice.CustomerName = item.CustomerName;
					invoice.CustomerNumber = item.CustomerNumber;

					invoice.DateCreated = date;
					invoice.DateModified = date;
					invoice.CreatedBy = modifiedBy;
					invoice.ModifiedBy = modifiedBy;
					invoice.IsActive = true;

					await _context.Invoices.AddAsync(invoice);
					await _context.SaveChangesAsync();

					var adjustment = new Adjustment();

					adjustment.InvoiceId = invoice.Id;
					adjustment.RequestId = requestId;
					adjustment.AdjustmentAmount = item.DiscountValue * item.InvoiceAmount;
					adjustment.AdjustmentTypeId = cashDiscountTypeId;
					adjustment.DiscountPercentage = item.DiscountValue * 100;
					adjustment.Remarks = item.Remarks;

					adjustment.DateCreated = date;
					adjustment.DateModified = date;
					adjustment.CreatedBy = modifiedBy;
					adjustment.ModifiedBy = modifiedBy;
					adjustment.IsActive = true;

					await _context.Adjustments.AddAsync(adjustment);
					await _context.SaveChangesAsync();
				}

				await dbTransaction.CommitAsync();
			}
			catch
			{
				await dbTransaction.RollbackAsync();
				throw;
			}
		}

		public async Task CreateApproveTransaction(long requestId, string createdBy)
		{
			await using var dbTransaction = await _context.Database.BeginTransactionAsync();

			try
			{
				bool isApprovable = await _requestRepo.IsApprovable(requestId);
				Guards.ThrowInvalidOperationIf(!isApprovable, Exceptions.ALREADY_APPROVED);

				var transaction = new TransactionCreateDto(requestId, "Approved");
				var transactId = await _transactionRepo.CreateAsync(transaction, createdBy);
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

		public async Task CreateDeclineTransaction(long requestId, string createdBy)
		{
			await using var dbTransaction = await _context.Database.BeginTransactionAsync();

			try
			{
				bool isDeclinable = await _requestRepo.IsDeclinable(requestId);
				Guards.ThrowInvalidOperationIf(!isDeclinable, Exceptions.ALREADY_DECLINED);

				var transaction = new TransactionCreateDto(requestId, "Declined");
				var transactId = await _transactionRepo.CreateAsync(transaction, createdBy);

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

		public async Task<bool> IsValid(CashDiscountCreateValidationDto cashCreateValidationRequest)
		{
			return await _invoiceRepo.IsInvoiceNumberAvailable(cashCreateValidationRequest.InvoiceNumber, "CDR");
		}
	}
}
