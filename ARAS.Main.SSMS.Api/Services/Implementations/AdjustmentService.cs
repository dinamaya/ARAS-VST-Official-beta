using ARAS.Main.SSMS.Api.App_Code.Globals;
using ARAS.Main.SSMS.Api.App_Code.Globals.Constants;
using ARAS.Main.SSMS.Api.Context;
using ARAS.Main.SSMS.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.Models.Entities;
using ARAS.Main.SSMS.Api.Repositories.Implementations;
using ARAS.Main.SSMS.Api.Repositories.Interfaces;
using ARAS.Main.SSMS.Api.Services.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ARAS.Main.SSMS.Api.Services.Implementations
{
	public class AdjustmentService : IAdjustmentService
	{
		private readonly MainDbContext _context;
		private readonly IRequestRepository _requestRepo;
		private readonly ITransactionRepository _transactionRepo;

        public AdjustmentService(MainDbContext context, IRequestRepository requestRepo, ITransactionRepository transactionRepo)
        {
            _context = context;
            _requestRepo = requestRepo;
            _transactionRepo = transactionRepo;
        }

        public async Task Approve(IEnumerable<long> requestIds, string createdBy)
		{
			await using var dbTransaction = await _context.Database.BeginTransactionAsync();

			try
			{
				IList<TransactionCreateDto> transactions = [];

				foreach (var requestId in requestIds)
				{
					bool isApprovable = await _requestRepo.IsApprovable(requestId);
					Guards.ThrowInvalidOperationIf(!isApprovable, Exceptions.ALREADY_APPROVED);
					transactions.Add(new TransactionCreateDto(requestId, "Approved"));
				}

				await _transactionRepo.CreateAsync(transactions, createdBy);
				await dbTransaction.CommitAsync();
			}
			catch
			{
				await dbTransaction.RollbackAsync();
				throw;
			}
		}

		public async Task Decline(IEnumerable<long> requestIds, string createdBy)
		{
			await using var dbTransaction = await _context.Database.BeginTransactionAsync();

			try
			{
				IList<TransactionCreateDto> transactions = [];

				foreach (var requestId in requestIds)
				{
					bool isDeclinable = await _requestRepo.IsDeclinable(requestId);
					Guards.ThrowInvalidOperationIf(!isDeclinable, Exceptions.ALREADY_DECLINED);
					transactions.Add(new TransactionCreateDto(requestId, "Declined"));
				}

				await _transactionRepo.CreateAsync(transactions, createdBy);
				await dbTransaction.CommitAsync();
			}
			catch
			{
				await dbTransaction.RollbackAsync();
				throw;
			}
		}

		public async Task Reject(IEnumerable<long> requestIds, string createdBy)
		{
			await using var dbTransaction = await _context.Database.BeginTransactionAsync();

			try
			{
				IList<TransactionCreateDto> transactions = [];

				foreach (var requestId in requestIds)
				{
					bool isRejectable = await _requestRepo.IsRejectable(requestId);
					Guards.ThrowInvalidOperationIf(!isRejectable, Exceptions.ALREADY_REJECT);
					transactions.Add(new TransactionCreateDto(requestId, "Rejected"));
				}

				await _transactionRepo.CreateAsync(transactions, createdBy);
				await dbTransaction.CommitAsync();
			}
			catch
			{
				await dbTransaction.RollbackAsync();
				throw;
			}
		}
	}
}
