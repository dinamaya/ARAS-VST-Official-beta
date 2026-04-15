using ARAS.Main.SSMS.Api.App_Code.Globals.Constants;
using ARAS.Main.SSMS.Api.Context;
using ARAS.Main.SSMS.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.Models.Entities;
using ARAS.Main.SSMS.Api.Repositories.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace ARAS.Main.SSMS.Api.Repositories.Implementations
{
	public class TransactionRepository(MainDbContext context, IStatusRepository statusRepo) : ITransactionRepository
	{
        private sealed class LatestQueueTransactionRow
        {
            public long TransactionId { get; set; }
            public long RequestId { get; set; }
            public string Creator { get; set; } = string.Empty;
            public DateTime DateCreated { get; set; }
            public string Description { get; set; } = string.Empty;
            public string AttachmentName { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public string AccountRole { get; set; } = string.Empty;
            public string Activity { get; set; } = string.Empty;
        }

		public async Task<long> CreateAsync(TransactionCreateDto data, string createdBy)
		{
			string statusId = await statusRepo.GetIdByName(data.StatusName);

            var transaction = new Transaction
            {
                RequestId = data.RequestId,
                StatusId = statusId,

                CreatedBy = createdBy,
                DateCreated = DateTime.Now,

                IsActive = true
            };

            await context.Transactions.AddAsync(transaction);
			await context.SaveChangesAsync();

			return transaction.Id;
		}

		public async Task<IEnumerable<long>> CreateAsync(IEnumerable<TransactionCreateDto> data, string createdBy)
		{
			IList<Transaction> results = [];
			string statusId = await statusRepo.GetIdByName(data.First().StatusName);

			var date = DateTime.Now;
			foreach (var t in data)
			{
				var _data = new Transaction()
				{
					RequestId = t.RequestId,
					StatusId = statusId,

					CreatedBy = createdBy,
					DateCreated = date,

					IsActive = true
				};
				results.Add(_data);
			}

			await context.Transactions.AddRangeAsync(results);
			await context.SaveChangesAsync();

			return results.Select(t => t.Id);
		}

		public async Task<IEnumerable<TransactionHistoryDto>> GetHistoryByRequestId(long requestId)
		{
			return await context.VwTransactionsHistory
				.AsNoTracking()
				.Where(t => t.RequestId == requestId)
				.OrderBy(t => t.DateCreated)
				.ThenBy(t => t.TransactionId)
				.Select(t => new TransactionHistoryDto
				{
					TransactionId = t.TransactionId,
					Creator = t.LastName + ", " + t.FirstName,
					DateCreated = t.DateCreated.ToString(Formats.Date.DISPLAY_COMPLETE),
					Description = t.Description,
					AttachmentName = t.AttachmentName,
					Status = t.Status,
					AccountRole = t.AccountRole
				}).ToListAsync();
		}


        public async Task<IEnumerable<TransactionHistoryDto>> GetLatestTransactions(string userId, string role, int count)
        {
            IQueryable<LatestQueueTransactionRow> query = IsApprovalRole(role)
                ? GetLatestTransactionsForApprover(role)
                : GetLatestTransactionsForRequestor(userId);

            return await query
                .OrderByDescending(x => x.DateCreated)
                .Take(count)
                .Select(x => new TransactionHistoryDto
                {
                    TransactionId = x.TransactionId,
                    RequestNumber = x.RequestId.ToString(),
                    Creator = x.Creator,
                    DateCreated = x.DateCreated.ToString(Formats.Date.DISPLAY_COMPLETE),
                    Description = x.Description,
                    AttachmentName = x.AttachmentName,
                    Status = x.Status,
                    AccountRole = x.AccountRole,
                    Activity = x.Activity
                })
                .ToListAsync();
        }

		public async Task<IEnumerable<EmailTimelineDetailsDto>> GetEmailHistoryByRequestId(long requestId)
		{
			var history = await context.VwTransactionsHistory
				.AsNoTracking()
				.Where(t => t.RequestId == requestId)
				.OrderBy(t => t.DateCreated)
				.ThenBy(t => t.TransactionId)
				.Select(t => new EmailTimelineDetailsDto
				{
					CreatorAction = t.Status,
					CreatorFullName = t.LastName + ", " + t.FirstName,
					Remarks = t.Description,
					DateCreated = t.DateCreated.ToString(Formats.Date.DISPLAY_COMPLETE),
				}).ToListAsync();

			return history.Select((t, i) => new EmailTimelineDetailsDto
			{
				CreatorAction = t.CreatorAction == "For CNC Approval" ? (i > 1 ? "Updated by" : "Requested By") : t.CreatorAction + " by",
				CreatorFullName = t.CreatorFullName,
				Remarks = t.Remarks,
				DateCreated = t.DateCreated,
			}).ToList();
		}

        private IQueryable<LatestQueueTransactionRow> GetLatestTransactionsForRequestor(string userId) =>
            context.VwTransactionsHistory
                .AsNoTracking()
                .Join(context.Requests,
                    t => t.RequestId,
                    r => r.Id,
                    (t, r) => new { t, r })
                .Where(x => x.r.CreatedBy == userId)
                .Select(x => new LatestQueueTransactionRow
                {
                    TransactionId = x.t.TransactionId,
                    RequestId = x.t.RequestId,
                    Creator = x.t.LastName + ", " + x.t.FirstName,
                    DateCreated = x.t.DateCreated,
                    Description = x.t.Description,
                    AttachmentName = x.t.AttachmentName,
                    Status = x.t.Status,
                    AccountRole = x.t.AccountRole,
                    Activity = x.t.Activity
                });

        private IQueryable<LatestQueueTransactionRow> GetLatestTransactionsForApprover(string role)
        {
            string queueStatus = GetApprovalQueueStatus(role);

            IQueryable<LatestQueueTransactionRow> queueRows =
                context.VwLatestReceiptAdjustmentDetails
                    .AsNoTracking()
                    .Where(x => x.Status == queueStatus)
                    .Select(x => new LatestQueueTransactionRow
                    {
                        TransactionId = x.TransactionId,
                        RequestId = x.RequestId,
                        Creator = (x.UpdaterLastName ?? string.Empty) + ", " + (x.UpdaterFirstName ?? string.Empty),
                        DateCreated = x.DateCreated,
                        Description = string.Empty,
                        AttachmentName = string.Empty,
                        Status = x.Status,
                        AccountRole = role,
                        Activity = x.AdjustmentType
                    })
                    .Concat(
                        context.VwLatestInvoiceAdjustments
                            .AsNoTracking()
                            .Where(x => x.Status == queueStatus)
                            .Select(x => new LatestQueueTransactionRow
                            {
                                TransactionId = x.TransactionId,
                                RequestId = x.RequestId,
                                Creator = (x.UpdaterLastName ?? string.Empty) + ", " + (x.UpdaterFirstName ?? string.Empty),
                                DateCreated = x.DateCreated,
                                Description = string.Empty,
                                AttachmentName = string.Empty,
                                Status = x.Status,
                                AccountRole = role,
                                Activity = x.AdjustmentType
                            }));

            return queueRows;
        }

        private static bool IsApprovalRole(string role) =>
            string.Equals(role, "CNC Approver", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(role, "FSG Validator", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(role, "FSG Approver", StringComparison.OrdinalIgnoreCase);

        private static string GetApprovalQueueStatus(string role) =>
            role switch
            {
                var currentRole when string.Equals(currentRole, "CNC Approver", StringComparison.OrdinalIgnoreCase) => "For CNC Approval",
                var currentRole when string.Equals(currentRole, "FSG Validator", StringComparison.OrdinalIgnoreCase) => "For FSG Validation",
                var currentRole when string.Equals(currentRole, "FSG Approver", StringComparison.OrdinalIgnoreCase) => "For FSG Approval",
                _ => throw new InvalidOperationException(Exceptions.INVALID_ROLE)
            };
    }
}
