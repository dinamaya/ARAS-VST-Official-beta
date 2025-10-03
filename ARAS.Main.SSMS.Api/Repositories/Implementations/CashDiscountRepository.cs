using ARAS.Main.Oracle.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.App_Code.Globals.Constants;
using ARAS.Main.SSMS.Api.Context;
using ARAS.Main.SSMS.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.Models.Entities;
using ARAS.Main.SSMS.Api.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ARAS.Main.SSMS.Api.Repositories.Implementations
{
	public class CashDiscountRepository : ICashDiscountRepository
	{
		private readonly MainDbContext _context;
		private readonly IStatusRepository _statusRepo;
		private readonly IRequestRepository _requestRepo;

		public CashDiscountRepository(MainDbContext context, IStatusRepository statusRepo, IRequestRepository requestRepo)
		{
			_context = context;
			_statusRepo = statusRepo;
			_requestRepo = requestRepo;
		}

		public string InsertedId { get; set; }

		public async Task CreateAsync(IEnumerable<CashDiscountCreateDto> data, string createdBy)
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

				var request = new Request();
				request.RequestNumber = $"CCGROUP-CDR-{DateTime.Now.ToString("ddMMyyyy")}-{Guid.NewGuid()}";
				request.AdjustmentTypeId = cashDiscountTypeId;
				request.RequestorId = createdBy;
				request.DateRequested = date;

				request.DateModified = date;
				request.ModifiedBy = createdBy;
				request.IsActive = true;

				await _context.Requests.AddAsync(request);
				await _context.SaveChangesAsync();

				var transaction = new Transaction();
				transaction.RequestId = request.Id;
				transaction.StatusId = pendingStatusId;

				transaction.DateCreated = DateTime.Now;
				transaction.IsActive = true;

				await _context.Transactions.AddAsync(transaction);
				await _context.SaveChangesAsync();

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
					invoice.CreatedBy = createdBy;
					invoice.ModifiedBy = createdBy;
					invoice.IsActive = true;

					await _context.Invoices.AddAsync(invoice);
					await _context.SaveChangesAsync();

					var adjustment = new Adjustment();
					adjustment.InvoiceId = invoice.Id;
					adjustment.RequestId = request.Id;
					adjustment.AdjustmentAmount = item.DiscountValue * item.InvoiceAmount;
					adjustment.AdjustmentAmount = item.DiscountValue * item.InvoiceAmount;
					adjustment.AdjustmentTypeId = cashDiscountTypeId;
					adjustment.DiscountPercentage = item.DiscountValue * 100;
					adjustment.Remarks = item.Remarks;

					adjustment.DateCreated = date;
					adjustment.DateModified = date;
					adjustment.CreatedBy = createdBy;
					adjustment.ModifiedBy = createdBy;
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
		
		public async Task<IEnumerable<TransactionRequestRowDto>> GetAllSubmissions()
		{
			return await _context.LatestTransactionRequestsVs
				.OrderByDescending(t => t.TransactionId)
				.Select(t => new TransactionRequestRowDto()
				{
					RequestId = t.RequestId,
					RequestNumber = t.RequestNumber,
					Requestor = ValidateFullName(t.RequestorFirstName, t.RequestorLastName),
					DateRequested = ((DateTime)t.DateRequested).ToString(Formats.Date.DISPLAY_COMPLETE),

					Approver = ValidateFullName(t.ApproverLastName, t.ApproverFirstName),
					DateApproved = t.DateApproved.HasValue ? ((DateTime)t.DateApproved).ToString(Formats.Date.DISPLAY_COMPLETE) : string.Empty,

					Validator = ValidateFullName(t.ValidatorLlastName, t.ValidatorFirstName),
					DateValidated = t.DateValidated.HasValue ? ((DateTime)t.DateValidated).ToString(Formats.Date.DISPLAY_COMPLETE) : string.Empty,

					Checker = ValidateFullName(t.CheckerLastName, t.CheckerFirstName),
					DateChecked = t.DateChecked.HasValue ? ((DateTime)t.DateChecked).ToString(Formats.Date.DISPLAY_COMPLETE) : string.Empty,

					Status = t.Status,
				})
				.ToListAsync();
		}

		public async Task<IEnumerable<TransactionRequestRowDto>> GetAllForApprovals()
		{
			return await _context.LatestTransactionRequestsVs
				.Where(t =>
					t.Status == "Pending" &&
					string.IsNullOrEmpty(t.ApproverId) && !t.DateApproved.HasValue &&
					string.IsNullOrEmpty(t.ValidatorId) && !t.DateValidated.HasValue)
				.OrderByDescending(t => t.TransactionId)
				.Select(t => new TransactionRequestRowDto()
				{
					RequestId = t.RequestId,
					RequestNumber = t.RequestNumber,
					Requestor = ValidateFullName(t.RequestorFirstName, t.RequestorLastName),
					DateRequested = ((DateTime)t.DateRequested).ToString(Formats.Date.DISPLAY_COMPLETE),

					Approver = ValidateFullName(t.ApproverLastName, t.ApproverFirstName),
					DateApproved = t.DateApproved.HasValue ? ((DateTime)t.DateApproved).ToString(Formats.Date.DISPLAY_COMPLETE) : string.Empty,

					Validator = ValidateFullName(t.ValidatorLlastName, t.ValidatorFirstName),
					DateValidated = t.DateValidated.HasValue ? ((DateTime)t.DateValidated).ToString(Formats.Date.DISPLAY_COMPLETE) : string.Empty,

					Checker = ValidateFullName(t.CheckerLastName, t.CheckerFirstName),
					DateChecked = t.DateChecked.HasValue ? ((DateTime)t.DateChecked).ToString(Formats.Date.DISPLAY_COMPLETE) : string.Empty,

					Status = t.Status,
				})
				.ToListAsync();
		}

		public async Task<IEnumerable<TransactionRequestRowDto>> GetAllForValidations()
		{
			return await _context.LatestTransactionRequestsVs
				.Where(t =>
					t.Status == "Approved" &&
					!string.IsNullOrEmpty(t.ApproverId) && t.DateApproved.HasValue &&
					string.IsNullOrEmpty(t.ValidatorId) && !t.DateValidated.HasValue)
				.OrderByDescending(t => t.TransactionId)
				.Select(t => new TransactionRequestRowDto()
				{
					RequestId = t.RequestId,
					RequestNumber = t.RequestNumber,
					Requestor = ValidateFullName(t.RequestorFirstName, t.RequestorLastName),
					DateRequested = ((DateTime)t.DateRequested).ToString(Formats.Date.DISPLAY_COMPLETE),

					Approver = ValidateFullName(t.ApproverLastName, t.ApproverFirstName),
					DateApproved = t.DateApproved.HasValue ? ((DateTime)t.DateApproved).ToString(Formats.Date.DISPLAY_COMPLETE) : string.Empty,

					Validator = ValidateFullName(t.ValidatorLlastName, t.ValidatorFirstName),
					DateValidated = t.DateValidated.HasValue ? ((DateTime)t.DateValidated).ToString(Formats.Date.DISPLAY_COMPLETE) : string.Empty,

					Checker = ValidateFullName(t.CheckerLastName, t.CheckerFirstName),
					DateChecked = t.DateChecked.HasValue ? ((DateTime)t.DateChecked).ToString(Formats.Date.DISPLAY_COMPLETE) : string.Empty,

					Status = t.Status,
				})
				.ToListAsync();
		}

		public async Task<IEnumerable<CashDiscountRowDto>> GetAdjustmentsByRequestId(long requestId)
		{
			return await _context.RequestAdjustmentVs
				.Where(r => r.AdjustmentActivity == "Cash Discount" && r.RequestId == requestId)
				.Select(r => new CashDiscountRowDto()
				{
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
			return await _context.LatestTransactionRequestsVs.Where(t => t.RequestId == requestId)
				.Select(t => new TransactionRequestRowDto()
				{
					RequestId = t.RequestId,
					RequestNumber = t.RequestNumber,
					Requestor = ValidateFullName(t.RequestorFirstName, t.RequestorLastName),
					DateRequested = ((DateTime)t.DateRequested).ToString(Formats.Date.DISPLAY_COMPLETE),

					Approver = ValidateFullName(t.ApproverLastName, t.ApproverFirstName),
					DateApproved = t.DateApproved.HasValue ? ((DateTime)t.DateApproved).ToString(Formats.Date.DISPLAY_COMPLETE) : string.Empty,

					Validator = ValidateFullName(t.ValidatorLlastName, t.ValidatorFirstName),
					DateValidated = t.DateValidated.HasValue ? ((DateTime)t.DateValidated).ToString(Formats.Date.DISPLAY_COMPLETE) : string.Empty,

					Checker = ValidateFullName(t.CheckerLastName, t.CheckerFirstName),
					DateChecked = t.DateChecked.HasValue ? ((DateTime)t.DateChecked).ToString(Formats.Date.DISPLAY_COMPLETE) : string.Empty,

					Status = t.Status,
				})
				.FirstOrDefaultAsync();
		}

		public async Task CreateApproveTransaction(long requestId, string createdBy)
		{
			bool isApprovable = await _requestRepo.IsApprovable(requestId);

			if (!isApprovable)
				throw new InvalidOperationException(Exceptions.ALREADY_APPROVED);

			var transactionRequest = await _context.Transactions
				.Where(t => t.RequestId == requestId)
				.OrderByDescending(t => t.Id)
				.FirstOrDefaultAsync();

			var approvedId = await _statusRepo.GetIdByName("Approved");

			var date = DateTime.UtcNow;
			var transaction = new Transaction();
			transaction.RequestId = requestId;

			transaction.ApproverId = createdBy;
			transaction.DateApproved = date;

			transaction.ValidatorId = transactionRequest.ValidatorId;
			transaction.DateValidated = transactionRequest.DateValidated;

			transaction.CheckerId = transactionRequest.CheckerId;
			transaction.DateChecked = transactionRequest.DateChecked;

			transaction.StatusId = approvedId;

			transaction.DateCreated = date;
			transaction.IsActive = true;

			await _context.Transactions.AddAsync(transaction);
			await _context.SaveChangesAsync();
		}

		public async Task CreateValidateTransaction(long requestId, string createdBy)
		{
			bool isValidatable = await _requestRepo.IsValidatable(requestId);

			if (!isValidatable)
				throw new InvalidOperationException(Exceptions.ALREADY_Validated);

			var transactionRequest = await _context.Transactions
				.Where(t => t.RequestId == requestId)
				.OrderByDescending(t => t.Id)
				.FirstOrDefaultAsync();

			var validatedId = await _statusRepo.GetIdByName("Validated");

			var date = DateTime.UtcNow;
			var transaction = new Transaction();
			transaction.RequestId = requestId;

			transaction.ApproverId = transactionRequest.ApproverId;
			transaction.DateApproved = transactionRequest.DateApproved;

			transaction.ValidatorId = createdBy;
			transaction.DateValidated = date;

			transaction.CheckerId = transactionRequest.CheckerId;
			transaction.DateChecked = transactionRequest.DateChecked;

			transaction.StatusId = validatedId;

			transaction.DateCreated = date;
			transaction.IsActive = true;

			await _context.Transactions.AddAsync(transaction);
			await _context.SaveChangesAsync();
		}

		private static string ValidateFullName(string fName, string lName) =>
			string.IsNullOrEmpty(lName) && string.IsNullOrEmpty(fName) ? string.Empty : lName + ", " + fName;
	}
}
