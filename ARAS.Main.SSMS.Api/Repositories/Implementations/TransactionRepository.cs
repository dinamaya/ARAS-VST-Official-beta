using ARAS.Main.SSMS.Api.App_Code.Globals.Constants;
using ARAS.Main.SSMS.Api.Context;
using ARAS.Main.SSMS.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.Models.Entities;
using ARAS.Main.SSMS.Api.Repositories.Interfaces;
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

		public async Task<IEnumerable<TransactionHistoryDto>> GetHistoryByRequestId(long requestId)
		{
			return await context.VwTransactionsHistory
				.AsNoTracking()
				.Where(t => t.RequestId == requestId)
				.Select(t => new TransactionHistoryDto
				{
					TransactionId = t.TransactionId,
					RequestNumber = t.RequestNumber,
					Creator = t.LastName + ", " + t.FirstName,
					DateCreated = t.DateCreated.ToString(Formats.Date.DISPLAY_COMPLETE),
					Description = t.Description,
					AttachmentName = t.AttachmentName,
					Status = t.Status,
					AccountRole = t.AccountRole
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
				CreatorAction = t.CreatorAction == "Pending" ? (i > 1 ? "Updated by" : "Requested By") : t.CreatorAction + " by",
				CreatorFullName = t.CreatorFullName,
				Remarks = t.Remarks,
				DateCreated = t.DateCreated,
			}).ToList();
		}
	}
}
