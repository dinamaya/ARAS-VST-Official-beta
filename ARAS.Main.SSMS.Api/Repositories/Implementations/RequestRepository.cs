using ARAS.Main.SSMS.Api.App_Code.Globals.Constants;
using ARAS.Main.SSMS.Api.Context;
using ARAS.Main.SSMS.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.Models.Entities;
using ARAS.Main.SSMS.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ARAS.Main.SSMS.Api.Repositories.Implementations
{
	public class RequestRepository : IRequestRepository
	{
		private readonly MainDbContext _context;

		public RequestRepository(MainDbContext context) => _context = context;

		public async Task<Request> GetById(long id) => await _context.Requests.FindAsync(id) ?? throw new InvalidOperationException(Exceptions.NOTFOUND_REQUEST);
		
		/// <summary>
		/// Checks if the request is approvable and not yet validated
		/// </summary>
		/// <param name="requestId"></param>
		/// <returns></returns>
		public async Task<bool> IsApprovable(long requestId)
		{
			return await _context.VwLatestRequestTransactions.AsNoTracking().AnyAsync(t =>
				t.RequestId == requestId &&
				t.Status == "Pending" &&
				t.ApproverId == null && t.ValidatorId == null
			);
		}

		/// <summary>
		/// Checks if the request is approvable and not yet validated
		/// </summary>
		/// <param name="requestId"></param>
		/// <returns></returns>
		public async Task<bool> IsDeclinable(long requestId)
		{
			var latest = await _context.VwLatestRequestTransactions.AsNoTracking().Where(t =>
				t.RequestId == requestId &&
				!(t.Status == "Rejected" || t.Status == "Declined" || t.Status == "Validated")
			).ToListAsync();

			return latest.Count() > 0;
		}

		/// <summary>
		/// Checks if the request is validatable and is approved
		/// </summary>
		/// <param name="requestId"></param>
		/// <returns></returns>
		public async Task<bool> IsValidatable(long requestId)
		{
			return await _context.VwLatestRequestTransactions.AsNoTracking().AnyAsync(t =>
				t.RequestId == requestId &&
				(t.Status == "Approved" || t.Status == "Pending") &&
				t.ApproverId != null && t.ValidatorId == null
			);
		}

		/// <summary>
		/// Checks if the request is rejectable
		/// </summary>
		/// <param name="requestId"></param>
		/// <returns></returns>
		public async Task<bool> IsRejectable(long requestId)
		{
			return await _context.VwLatestRequestTransactions.AsNoTracking().AnyAsync(t =>
				t.RequestId == requestId &&
				!(t.Status == "Rejected" || t.Status == "Declined")
			);
		}

		/// <summary>
		/// Checks if the request is currently declined
		/// </summary>
		/// <param name="requestId"></param>
		/// <returns></returns>
		public async Task<bool> IsDeclined(long requestId) => await _context.VwLatestRequestTransactions.AsNoTracking().AnyAsync(t => t.RequestId == requestId && t.Status == "Declined");

		public async Task<long> CreateAsync(RequestCreateDto data, string createdBy)
		{
			var request = new Request();
			request.RequestNumber = data.RequestNumber;
			request.AdjustmentTypeId = data.AdjustmentTypeId;
			request.CreatedBy = createdBy;
			request.DateCreated = DateTime.Now;

			await _context.Requests.AddAsync(request);
			await _context.SaveChangesAsync();

			return request.Id;
		}

		public async Task<string> GetRequestNumberById(long requestId)
		{
			return await _context.Requests
				.AsNoTracking()
				.Where(r => r.Id == requestId)
				.Select(r => r.RequestNumber)
				.FirstOrDefaultAsync() ?? throw new InvalidOperationException(Exceptions.NOTFOUND_REQUEST);
		}

		public async Task<RequestUpdateEmailDetailsDto> GetForEmailDetailsById(long requestId)
		{
			return await _context.VwLatestRequestTransactions
				.AsNoTracking()
				.Where(r => r.RequestId == requestId)
				.Select(r => new RequestUpdateEmailDetailsDto
				{
					RequestNumber = r.RequestNumber,
					Creator = r.RequestorFirstName + " " + r.RequestorLastName
				})
				.FirstOrDefaultAsync() ?? throw new InvalidOperationException(Exceptions.NOTFOUND_REQUEST);

		}

		public async Task<IEnumerable<TransactionRequestRowDto>> GetAllSubmissionsByType(string adjustmentTypeCode)
		{
			adjustmentTypeCode = adjustmentTypeCode.ToUpper();
			return await _context.VwLatestRequestTransactions
				.AsNoTracking()
				.OrderByDescending(t => t.TransactionId)
				.Where(t => t.AdjustmentTypeCode == adjustmentTypeCode)
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

		public async Task<IEnumerable<TransactionRequestRowDto>> GetAllForApprovalsByType(string adjustmentTypeCode)
		{
			adjustmentTypeCode = adjustmentTypeCode.ToUpper();
			return await _context.VwLatestRequestTransactions
				.AsNoTracking()
				.Where(t =>
					t.Status == "Pending" &&
					t.ApproverId == null && t.ValidatorId == null &&
					t.AdjustmentTypeCode == adjustmentTypeCode
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

		public async Task<IEnumerable<TransactionRequestRowDto>> GetAllForValidationsByType(string adjustmentTypeCode)
		{
			adjustmentTypeCode = adjustmentTypeCode.ToUpper();
			return await _context.VwLatestRequestTransactions
				.AsNoTracking()
				.Where(t =>
					(t.Status == "Approved" || t.Status == "Pending") &&
					t.ApproverId != null && t.ValidatorId == null &&
					t.AdjustmentTypeCode == adjustmentTypeCode
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

		public async Task<TransactionRequestRowDto> GetTransactionRequestByRequestId(long requestId)
		{
			return await _context.VwLatestRequestTransactions
				.AsNoTracking()
				.Where(t => t.RequestId == requestId)
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

        public async Task<ReportsDto> GetTransactionRequestForReport()
        {
            return await _context.VwLatestRequestTransactions
				.OrderByDescending(t => t.DateRequested)
				.Select(t => new ReportsDto
				{
					RequestNumber = t.RequestNumber ?? string.Empty,
					AdjustmentTypeCode = t.AdjustmentTypeCode ?? string.Empty,
					DateRequested = t.DateRequested,
					Status = t.Status ?? string.Empty
				})
				.FirstOrDefaultAsync() ?? new ReportsDto();
        }

        private static string ValidateFullName(string fName, string lName) =>
			string.IsNullOrEmpty(lName) && string.IsNullOrEmpty(fName) ? string.Empty : lName + ", " + fName;

	}
}
