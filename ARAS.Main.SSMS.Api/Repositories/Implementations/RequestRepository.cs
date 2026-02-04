using ARAS.Main.SSMS.Api.App_Code.Globals;
using ARAS.Main.SSMS.Api.App_Code.Globals.Constants;
using ARAS.Main.SSMS.Api.Context;
using ARAS.Main.SSMS.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.Models.Entities;
using ARAS.Main.SSMS.Api.Models.Views;
using ARAS.Main.SSMS.Api.Repositories.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Runtime.InteropServices;

namespace ARAS.Main.SSMS.Api.Repositories.Implementations
{
	public class RequestRepository : IRequestRepository
	{
		private readonly MainDbContext _context;
		private readonly ITransactionRepository _transactionRepo;

        public RequestRepository(MainDbContext context, ITransactionRepository transactionRepo)
        {
            _context = context;
            _transactionRepo = transactionRepo;
        }

        public async Task<Request> GetById(long id) => await _context.Requests.FindAsync(id) ?? throw new InvalidOperationException(Exceptions.NOTFOUND_REQUEST);

		/// <summary>
		/// Checks if the request is updatable
		/// </summary>
		/// <param name="requestId"></param>
		/// <returns></returns>
		public async Task<bool> IsUpdatable(long requestId)
		{
			return await _context.VwAllAdjustmentRequestLatestStatus.AsNoTracking().AnyAsync(t =>
				t.RequestId == requestId &&
				(t.Status == "Pending" || t.Status == "Resubmitted" || t.Status == "Declined")
			);
		}

		/// <summary>
		/// Checks if the request is approvable and not yet validated
		/// </summary>
		/// <param name="requestId"></param>
		/// <returns></returns>
		public async Task<bool> IsApprovable(long requestId)
		{
			return await _context.VwAllAdjustmentRequestLatestStatus.AsNoTracking().AnyAsync(t =>
				t.RequestId == requestId &&
				(t.Status == "Pending" || t.Status == "Resubmitted")
			);
		}

		/// <summary>
		/// Checks if the request is approvable and not yet validated
		/// </summary>
		/// <param name="requestId"></param>
		/// <returns></returns>
		public async Task<bool> IsDeclinable(long requestId)
		{
			var latest = await _context.VwAllAdjustmentRequestLatestStatus.AsNoTracking().Where(t =>
				t.RequestId == requestId &&
				!(t.Status == "Rejected" || t.Status == "Declined" || t.Status == "Approved" || t.Status == "Posted")
			).ToListAsync();

			return latest.Count() > 0;
		}

		/// <summary>
		/// Checks if the request is rejectable
		/// </summary>
		/// <param name="requestId"></param>
		/// <returns></returns>
		public async Task<bool> IsRejectable(long requestId)
		{
			return await _context.VwLatestReceiptAdjustmentDetails.AsNoTracking().AnyAsync(t =>
				t.RequestId == requestId &&
				!(t.Status == "Rejected" || t.Status == "Declined" || t.Status == "Approved" || t.Status == "Posted")
			);
		}

		/// <summary>
		/// Checks if the request is rejectable
		/// </summary>
		/// <param name="requestId"></param>
		/// <returns></returns>
		public async Task<bool> IsPostable(long requestId) => 
			await _context.VwLatestReceiptAdjustmentDetails.AsNoTracking().AnyAsync(t =>t.RequestId == requestId && t.Status != "Posted");

		/// <summary>
		/// Checks if the request is currently declined
		/// </summary>
		/// <param name="requestId"></param>
		/// <returns></returns>
		public async Task<bool> IsDeclined(long requestId) => await _context.VwLatestReceiptAdjustmentDetails.AsNoTracking().AnyAsync(t => t.RequestId == requestId && t.Status == "Declined");

		public async Task<long> CreateAsync(string adjustmentTypeId, string createdBy)
		{
            var request = new Request
            {
                AdjustmentTypeId = adjustmentTypeId,
                CreatedBy = createdBy,
                DateCreated = DateTime.Now
            };

            await _context.Requests.AddAsync(request);
			await _context.SaveChangesAsync();

			return request.Id;
		}

		public async Task<IEnumerable<long>> CreateAsync(IEnumerable<string> adjustmentTypeIds, string createdBy)
		{
			IList<Request> results = [];
			var date = DateTime.Now;
			foreach (string id in adjustmentTypeIds)
			{
				results.Add(new Request
				{
					AdjustmentTypeId = id,
					CreatedBy = createdBy,
					DateCreated = date
				});
			}

			await _context.Requests.AddRangeAsync(results);
			await _context.SaveChangesAsync();

			return results.Select(r => r.Id);
		}

		public async Task<string> GetRequestNumberById(long requestId)
		{
			return await _context.Requests
				.AsNoTracking()
				.Where(r => r.Id == requestId)
				.Select(r => r.Id.ToString())
				.FirstOrDefaultAsync() ?? throw new InvalidOperationException(Exceptions.NOTFOUND_REQUEST);
		}

		public async Task<RequestUpdateEmailDetailsDto> GetForEmailDetailsById(long requestId)
		{
			return await _context.VwLatestReceiptAdjustmentDetails
				.AsNoTracking()
				.Where(r => r.RequestId == requestId)
				.Select(r => new RequestUpdateEmailDetailsDto
				{
					RequestNumber = r.RequestId.ToString(),
					Creator = r.RequestorFirstName + " " + r.RequestorLastName
				})
				.FirstOrDefaultAsync() ?? throw new InvalidOperationException(Exceptions.NOTFOUND_REQUEST);

		}


		public async Task<IEnumerable<InvoiceAdjustmentRowDto>> GetInvoicedjustmentApprovals(SearchRequestDto data)
		{
			data.Value = data.Value.ToUpper();

			IQueryable<LatestInvoiceAdjustmentsV> query = GetInvoiceAdjustmentsForApprovals();
			query = data.Category.ToUpper() switch
			{
				"INVOICE NUMBER" => GetByInvoiceNumber(query, data.Value),
				"CUSTOMER NAME" => GetByCustomerName(query, data.Value),
				"ADJUSTMENT TYPE" => GetByAdjustmentType(query, data.Value),
				"REQUESTOR NAME" => GetByRequestor(query, data.Value),
				_ => throw new InvalidOperationException(Exceptions.INVALID_SEARCHCATEGORY),
			};

			query = FilterByDateRange(query, data.StartDate.ToDateTime(TimeOnly.MinValue), data.EndDate.ToDateTime(TimeOnly.MaxValue));
			return await ToRequestAdjustmentRow(query);
		}

		public async Task<IEnumerable<InvoiceAdjustmentRowDto>> GetInvoiceAdjustmentSubmissions(SearchRequestDto data, string role, string fullname)
		{
			data.Value = data.Value.ToUpper();

			IQueryable<LatestInvoiceAdjustmentsV> query = GetInvoiceAdjustmentsSubmissions();
			query = data.Category.ToUpper() switch
			{
				"INVOICE NUMBER" => GetByInvoiceNumber(query, data.Value),
				"CUSTOMER NAME" => GetByCustomerName(query, data.Value),
				"ADJUSTMENT TYPE" => GetByAdjustmentType(query, data.Value),
				"REQUESTOR NAME" => GetByRequestor(query, data.Value),
				_ => throw new InvalidOperationException(Exceptions.INVALID_SEARCHCATEGORY),
			};

			query = FilterByDateRange(query, data.StartDate.ToDateTime(TimeOnly.MinValue), data.EndDate.ToDateTime(TimeOnly.MaxValue));
			query = FilterByRole(query, role, fullname);
			return await ToRequestAdjustmentRow(query);
		}

		public async Task<IEnumerable<ReceiptAdjustmentRowDto>> GetReceiptAdjustmentApprovals(SearchRequestDto data)
		{
			data.Value = data.Value.ToUpper();

			IQueryable<LatestReceiptAdjustmentDetailsV> query = GetReceiptAdjustmentsForApprovals();
            query = data.Category.ToUpper() switch
            {
                "INVOICE NUMBER" => GetByInvoiceNumber(query, data.Value),
                "CUSTOMER NAME" => GetByCustomerName(query, data.Value),
                "ADJUSTMENT TYPE" => GetByAdjustmentType(query, data.Value),
                "REQUESTOR NAME" => GetByRequestor(query, data.Value),
                _ => throw new InvalidOperationException(Exceptions.INVALID_SEARCHCATEGORY),
            };

			query = FilterByDateRange(query, data.StartDate.ToDateTime(TimeOnly.MinValue), data.EndDate.ToDateTime(TimeOnly.MaxValue));
			return await ToRequestAdjustmentRow(query);
		}

		public async Task<IEnumerable<ReceiptAdjustmentRowDto>> GetReceiptAdjustmentSubmissions(SearchRequestDto data, string role, string fullname)
		{
			data.Value = data.Value.ToUpper();

			IQueryable<LatestReceiptAdjustmentDetailsV> query = GetReceiptAdjustmentsSubmissions();
			query = data.Category.ToUpper() switch
			{
				"INVOICE NUMBER" => GetByInvoiceNumber(query, data.Value),
				"CUSTOMER NAME" => GetByCustomerName(query, data.Value),
				"ADJUSTMENT TYPE" => GetByAdjustmentType(query, data.Value),
				"REQUESTOR NAME" => GetByRequestor(query, data.Value),
				_ => throw new InvalidOperationException(Exceptions.INVALID_SEARCHCATEGORY),
			};

			query = FilterByDateRange(query, data.StartDate.ToDateTime(TimeOnly.MinValue), data.EndDate.ToDateTime(TimeOnly.MaxValue));
			query = FilterByRole(query, role, fullname);
			return await ToRequestAdjustmentRow(query);
		}

		public async Task<TransactionRequestRowDto> GetTransactionRequestByRequestId(long requestId)
		{
			return await _context.VwLatestReceiptAdjustmentDetails
				.AsNoTracking()
				.Where(t => t.RequestId == requestId)
				.Select(t => new TransactionRequestRowDto()
				{
					RequestId = t.RequestId,
					RequestNumber = t.RequestId.ToString(),
					Requestor = ValidateFullName(t.RequestorFirstName, t.RequestorLastName),
					DateRequested = t.DateRequested.ToString(Formats.Date.DISPLAY_COMPLETE),

					Approver = ValidateFullName(t.ApproverFirstName, t.ApproverLastName),
					DateApproved = t.DateApproved.HasValue ? ((DateTime)t.DateApproved).ToString(Formats.Date.DISPLAY_COMPLETE) : string.Empty,

					Status = t.Status,
				})
				.FirstOrDefaultAsync();
		}

        private static string ValidateFullName(string fName, string lName) =>
			string.IsNullOrEmpty(lName) && string.IsNullOrEmpty(fName) ? string.Empty : lName + ", " + fName;

		private IQueryable<LatestReceiptAdjustmentDetailsV> GetReceiptAdjustmentsForApprovals() =>
			_context.VwLatestReceiptAdjustmentDetails.AsNoTracking()
				.Where(t => (t.Status == "Pending") || (t.Status == "Resubmitted") && t.ApproverId == null);


		private IQueryable<LatestInvoiceAdjustmentsV> GetInvoiceAdjustmentsForApprovals() =>
			_context.VwLatestInvoiceAdjustments.AsNoTracking()
				.Where(t => (t.Status == "Pending") || (t.Status == "Resubmitted") && t.ApproverId == null);

		private IQueryable<LatestInvoiceAdjustmentsV> GetInvoiceAdjustmentsSubmissions() =>
			_context.VwLatestInvoiceAdjustments.AsNoTracking();

		private IQueryable<LatestReceiptAdjustmentDetailsV> GetReceiptAdjustmentsSubmissions() =>
	_context.VwLatestReceiptAdjustmentDetails.AsNoTracking();

		private IQueryable<LatestReceiptAdjustmentDetailsV> GetByInvoiceNumber(IQueryable<LatestReceiptAdjustmentDetailsV> query, string invoiceNumber) =>
			query.Where(t => t.InvoiceNumber == invoiceNumber);

		private IQueryable<LatestReceiptAdjustmentDetailsV> GetByCustomerName(IQueryable<LatestReceiptAdjustmentDetailsV> query, string customerName) =>
			query.Where(t => t.CustomerName == customerName);

		private IQueryable<LatestReceiptAdjustmentDetailsV> GetByRequestor(IQueryable<LatestReceiptAdjustmentDetailsV> query, string requestorName) =>
			query.Where(t => (t.RequestorFirstName + " " + t.RequestorLastName).ToUpper() == requestorName);

		private IQueryable<LatestReceiptAdjustmentDetailsV> GetByAdjustmentType(IQueryable<LatestReceiptAdjustmentDetailsV> query, string adjustmentTypeCode) =>
			query.Where(t => t.AdjustmentType.ToUpper() == adjustmentTypeCode);

		private IQueryable<LatestReceiptAdjustmentDetailsV> FilterByDateRange(IQueryable<LatestReceiptAdjustmentDetailsV> query, DateTime start, DateTime end) => 
			query.Where(t => t.DateCreated >= start && t.DateCreated <= end);

		private IQueryable<LatestReceiptAdjustmentDetailsV> FilterByRole(IQueryable<LatestReceiptAdjustmentDetailsV> query, string role, string name)
		{
			name = name.ToUpper();
			switch (role.ToUpper())
			{
				case "REQUESTOR":
					return query.Where(t => (t.RequestorFirstName + " " + t.RequestorLastName).ToUpper() == name);
				case "VALIDATOR" or "APPROVER":
					return query;
				default:
					throw new Exception(Exceptions.INVALID_ROLE);
			}
		}


		private IQueryable<LatestInvoiceAdjustmentsV> GetByInvoiceNumber(IQueryable<LatestInvoiceAdjustmentsV> query, string invoiceNumber) =>
			query.Where(t => t.InvoiceNumber == invoiceNumber);

		private IQueryable<LatestInvoiceAdjustmentsV> GetByCustomerName(IQueryable<LatestInvoiceAdjustmentsV> query, string customerName) =>
			query.Where(t => t.CustomerName == customerName);

		private IQueryable<LatestInvoiceAdjustmentsV> GetByRequestor(IQueryable<LatestInvoiceAdjustmentsV> query, string requestorName) =>
			query.Where(t => (t.RequestorFirstName + " " + t.RequestorLastName).ToUpper() == requestorName);

		private IQueryable<LatestInvoiceAdjustmentsV> GetByAdjustmentType(IQueryable<LatestInvoiceAdjustmentsV> query, string adjustmentTypeCode) =>
			query.Where(t => t.AdjustmentType.ToUpper() == adjustmentTypeCode);

		private IQueryable<LatestInvoiceAdjustmentsV> FilterByDateRange(IQueryable<LatestInvoiceAdjustmentsV> query, DateTime start, DateTime end) =>
			query.Where(t => t.DateCreated >= start && t.DateCreated <= end);

		private IQueryable<LatestInvoiceAdjustmentsV> FilterByRole(IQueryable<LatestInvoiceAdjustmentsV> query, string role, string name)
		{
			name = name.ToUpper();
			switch (role.ToUpper())
			{
				case "REQUESTOR":
					return query.Where(t => (t.RequestorFirstName + " " + t.RequestorLastName).ToUpper() == name);
				case "VALIDATOR" or "APPROVER":
					return query;
				default:
					throw new Exception(Exceptions.INVALID_ROLE);
			}
		}

		private async Task<IEnumerable<InvoiceAdjustmentRowDto>> ToRequestAdjustmentRow(IQueryable<LatestInvoiceAdjustmentsV> query) =>
			(await query.Select(q => new InvoiceAdjustmentRowDto()
			{
				RequestId = q.RequestId,
				Requestor = ValidateFullName(q.RequestorFirstName, q.RequestorLastName),
				DateRequested = q.DateRequested.ToString(Formats.Date.DISPLAY_COMPLETE),
				Approver = ValidateFullName(q.ApproverFirstName, q.ApproverLastName),
				DateApproved = q.DateApproved.HasValue ? ((DateTime)q.DateApproved).ToString(Formats.Date.DISPLAY_COMPLETE) : string.Empty,
				Creator =  q.Status == "Posted" ? "SYSTEM" : ValidateFullName(q.UpdaterFirstName, q.UpdaterLastName),
				DateCreated = q.DateCreated.ToString(Formats.Date.DISPLAY_COMPLETE),
				Status = q.Status,
				CustomerName = q.CustomerName,
				AdjustmentType = q.AdjustmentType,
				InvoiceNumber = q.InvoiceNumber,
				ReferencesCount = "0" // Placeholder as ReferencesCount is not available in LatestReceiptAdjustmentDetailsV
			})
            .ToListAsync()).OrderByDescending(a => a.DateCreated);

		private async Task<IEnumerable<ReceiptAdjustmentRowDto>> ToRequestAdjustmentRow(IQueryable<LatestReceiptAdjustmentDetailsV> query) =>
			(await query.Select(q => new ReceiptAdjustmentRowDto()
			{
				RequestId = q.RequestId,
				Requestor = ValidateFullName(q.RequestorFirstName, q.RequestorLastName),
				DateRequested = q.DateRequested.ToString(Formats.Date.DISPLAY_COMPLETE),
				Approver = ValidateFullName(q.ApproverFirstName, q.ApproverLastName),
				DateApproved = q.DateApproved.HasValue ? ((DateTime)q.DateApproved).ToString(Formats.Date.DISPLAY_COMPLETE) : string.Empty,
				Creator =  q.Status == "Posted" ? "SYSTEM" : ValidateFullName(q.UpdaterFirstName, q.UpdaterLastName),
				DateCreated = q.DateCreated.ToString(Formats.Date.DISPLAY_COMPLETE),
				Status = q.Status,
				CustomerName = q.CustomerName,
				InvoiceNumber = q.InvoiceNumber,
				AdjustmentType = q.AdjustmentType,
				AdjustmentTypeCode = q.AdjustmentTypeCode.ToLower()
			})
            .ToListAsync()).OrderByDescending(a => a.DateCreated);

        public async Task Approve(long requestId, string modifiedBy)
        {
			await using var dbTransaction = await _context.Database.BeginTransactionAsync();

			try
			{
				bool isApprovable = await IsApprovable(requestId);
				Guards.ThrowInvalidOperationIf(!isApprovable, Exceptions.ALREADY_APPROVED);
				var transaction = new TransactionCreateDto(requestId, "Approved");

				await _transactionRepo.CreateAsync(transaction, modifiedBy);
				await dbTransaction.CommitAsync();
			}
			catch
			{
				await dbTransaction.RollbackAsync();
				throw;
			}
		}

        public async Task Decline(long requestId, string modifiedBy)
        {
			await using var dbTransaction = await _context.Database.BeginTransactionAsync();
			try
			{
				bool isDeclinable = await IsDeclinable(requestId);
				Guards.ThrowInvalidOperationIf(!isDeclinable, Exceptions.ALREADY_DECLINED);
				var transaction = new TransactionCreateDto(requestId, "Declined");

				await _transactionRepo.CreateAsync(transaction, modifiedBy);
				await dbTransaction.CommitAsync();
			}
			catch
			{
				await dbTransaction.RollbackAsync();
				throw;
			}
		}

        public async Task Reject(long requestId, string modifiedBy)
        {
			await using var dbTransaction = await _context.Database.BeginTransactionAsync();

			try
			{
				bool isRejectable = await IsRejectable(requestId);
				Guards.ThrowInvalidOperationIf(!isRejectable, Exceptions.ALREADY_REJECT);
				var transaction = new TransactionCreateDto(requestId, "Rejected");

				await _transactionRepo.CreateAsync(transaction, modifiedBy);
				await dbTransaction.CommitAsync();
			}
			catch
			{
				await dbTransaction.RollbackAsync();
				throw;
			}
		}

        public async Task<string> InvoiceExistingAdjustments(string invoiceNumber, IEnumerable<string> adjustmentTypeIds)
        {
            var adjustments = await _context.VwReceiptAdjustments.AsNoTracking().Where(t => t.InvoiceNumber == invoiceNumber).ToListAsync();
			string existingAdjustments = string.Empty;
			foreach(var adj in adjustments)
				if(adjustmentTypeIds.Any(a => a == adj.AdjustmentTypeId))
					existingAdjustments += adj.AdjustmentType + ", ";

			return existingAdjustments.Length < 1 ? string.Empty : existingAdjustments.Remove(existingAdjustments.Length - 2);
		}
    }
}
