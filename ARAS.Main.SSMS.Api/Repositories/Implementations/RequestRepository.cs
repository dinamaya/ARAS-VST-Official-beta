using ARAS.Main.SSMS.Api.Context;
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

		/// <summary>
		/// Checks if the request is approvable and not yet validated
		/// </summary>
		/// <param name="requestId"></param>
		/// <returns></returns>
		public async Task<bool> IsApprovable(long requestId)
		{
			return await _context.LatestTransactionRequestsVs.AnyAsync(t =>
				t.RequestId == requestId &&
				t.Status == "Pending" &&
				string.IsNullOrEmpty(t.ApproverId) && !t.DateApproved.HasValue &&
				string.IsNullOrEmpty(t.ValidatorId) && !t.DateValidated.HasValue
			);
		}

		/// <summary>
		/// Checks if the request is validatable and is approved
		/// </summary>
		/// <param name="requestId"></param>
		/// <returns></returns>
		public async Task<bool> IsValidatable(long requestId)
		{
			return await _context.LatestTransactionRequestsVs.AnyAsync(t =>
				t.RequestId == requestId &&
				t.Status == "Approved" &&
				!string.IsNullOrEmpty(t.ApproverId) && t.DateApproved.HasValue &&
				string.IsNullOrEmpty(t.ValidatorId) && !t.DateValidated.HasValue
			);
		}
	}
}
