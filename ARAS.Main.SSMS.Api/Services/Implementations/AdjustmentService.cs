using ARAS.Main.SSMS.Api.App_Code.Globals;
using ARAS.Main.SSMS.Api.App_Code.Globals.Constants;
using ARAS.Main.SSMS.Api.App_Code.Globals.Helpers;
using ARAS.Main.SSMS.Api.Context;
using ARAS.Main.SSMS.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.Repositories.Interfaces;
using ARAS.Main.SSMS.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ARAS.Main.SSMS.Api.Services.Implementations
{
	public class AdjustmentService : IAdjustmentService
	{
		private readonly MainDbContext _context;
		private readonly IRequestRepository _requestRepo;
		private readonly ITransactionRepository _transactionRepo;
		private readonly IRemarksRepository _remarksRepo;

		public AdjustmentService(MainDbContext context, IRequestRepository requestRepo, ITransactionRepository transactionRepo, IRemarksRepository remarksRepo)
        {
            _context = context;
            _requestRepo = requestRepo;
            _transactionRepo = transactionRepo;
			_remarksRepo = remarksRepo;
        }

        public async Task Approve(IEnumerable<long> requestIds, string createdBy, string role)
		{
			await using var dbTransaction = await _context.Database.BeginTransactionAsync();

			try
			{
				IList<TransactionCreateDto> transactions = [];
				requestIds = [.. requestIds.Distinct()];

				foreach (var requestId in requestIds)
				{
					string nextStatus = await _requestRepo.GetNextApprovalStatus(requestId, role);
					transactions.Add(new TransactionCreateDto(requestId, nextStatus));
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
			requestIds = [.. requestIds.Distinct()];
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
			requestIds = [.. requestIds.Distinct()];

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
