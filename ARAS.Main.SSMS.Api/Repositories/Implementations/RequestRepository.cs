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

		public RequestRepository(MainDbContext context)
		{
			_context = context;
		}

		public async Task<Request> GetById(long id) => await _context.Requests.FindAsync(id) ?? throw new InvalidOperationException(Exceptions.NOTFOUND_REQUEST);
		
		/// <summary>
		/// Checks if the request is approvable and not yet validated
		/// </summary>
		/// <param name="requestId"></param>
		/// <returns></returns>
		public async Task<bool> IsApprovable(long requestId)
		{
			return await _context.VwLatestRequestTransactions.AnyAsync(t =>
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
			var latest = await _context.VwLatestRequestTransactions.Where(t =>
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
			return await _context.VwLatestRequestTransactions.AnyAsync(t =>
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
			return await _context.VwLatestRequestTransactions.AnyAsync(t =>
				t.RequestId == requestId &&
				!(t.Status == "Rejected" || t.Status == "Declined")
			);
		}

		public async Task<long> CreateAsync(RequestCreateDto data, string createdBy)
		{
			var request = new Request();
			request.RequestNumber = data.RequestNumber;
			request.AdjustmentTypeId = data.AdjustmentTypeId;
			request.CreatedBy = createdBy;
			request.DateCreated = DateTime.UtcNow;

			await _context.Requests.AddAsync(request);
			await _context.SaveChangesAsync();

			return request.Id;
		}
	}
}
