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

			var transaction = new Transaction();

			transaction.RequestId = data.RequestId;
			transaction.StatusId = statusId;

			transaction.CreatedBy = createdBy;
			transaction.DateCreated = DateTime.UtcNow;

			transaction.IsActive = true;

			await context.Transactions.AddAsync(transaction);
			await context.SaveChangesAsync();

			return transaction.Id;
		}

		public async Task<IEnumerable<TransactionHistoryDto>> GetHistoryByRequestId(long requestId)
		{
			return await context.VwTransactionsHistory
				.Where(t => t.RequestId == requestId)
				.Select(t => new TransactionHistoryDto
				{
					TransactionId = t.TransactionId,
					RequestNumber = t.RequestNumber,
					Creator = t.LastName + ", " + t.FirstName,
					DateCreated = t.DateCreated.ToString(Formats.Date.DISPLAY_COMPLETE),
					Status = t.Status
				}).ToListAsync();
		}
	}
}
