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


        public async Task<IEnumerable<TransactionHistoryDto>> GetLatestTransactions(string requestorId, int count)
        {
            return await context.VwTransactionsHistory
                .AsNoTracking()
                .Join(context.Requests,
                      t => t.RequestId,
                      r => r.Id,
                      (t, r) => new { t, r })
                .Where(x => x.r.CreatedBy == requestorId)
                .OrderByDescending(x => x.t.DateCreated)
                .Take(count)
                .Select(x => new TransactionHistoryDto
                {
                    TransactionId = x.t.TransactionId,
                    RequestNumber = x.t.RequestId.ToString(),
                    Creator = x.t.LastName + ", " + x.t.FirstName,
                    DateCreated = x.t.DateCreated.ToString(Formats.Date.DISPLAY_COMPLETE),
                    Description = x.t.Description,
                    AttachmentName = x.t.AttachmentName,
                    Status = x.t.Status,
                    AccountRole = x.t.AccountRole,
					Activity = x.t.Activity
                }).ToListAsync();
        }

        public async Task<IEnumerable<EmailTimelineDetailsDto>> GetEmailHistoryByRequestId(long requestId)
		{
			var history = await context.VwTransactionsHistory
				.AsNoTracking()
				.Where(t => t.RequestId == requestId)
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
    }
}
