using ARAS.Main.SSMS.Api.Context;
using ARAS.Main.SSMS.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.Models.Entities;
using ARAS.Main.SSMS.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ARAS.Main.SSMS.Api.Repositories.Implementations
{
    public class APAROffsetRepository : IAPAROffsetRepository
    {
        private readonly MainDbContext _context;
        private readonly IRequestRepository _requestRepo;
        private readonly ITransactionRepository _transactionRepo;

        public APAROffsetRepository(MainDbContext context, IRequestRepository requestRepo, ITransactionRepository transactionRepo)
        {
            _context = context;
            _requestRepo = requestRepo;
            _transactionRepo = transactionRepo;
        }

        public async Task<long> CreateAsync(RequestCreationDto<AdjustmentRequestCreationDto<APAROffsetCreateDto>> data, string createdBy)
        {
            await using var dbTransaction = await _context.Database.BeginTransactionAsync();

            try
            {
                string aparOffsetTypeId = await _context.AdjustmentTypes.Where(x => x.Code.Equals("APAR")).Select(x => x.Id).FirstAsync();
                string referenceNo = "APAR-" + DateTime.Now.Ticks;

                var request = new RequestCreateDto(referenceNo, aparOffsetTypeId);
                var requestId = await _requestRepo.CreateAsync(request, createdBy);

                var transaction = new TransactionCreateDto(requestId, "Pending");
                var transactId = await _transactionRepo.CreateAsync(transaction, createdBy);

                foreach (var item in data.Model.Adjustments)
                {
                    var aparOffset = new APAROffset
                    {
                        RequestId = requestId,
                        InvoiceId = item.InvoiceId,
                        Amount = item.Amount,
                        Type = item.Type,
                        AdjustmentTypeId = aparOffsetTypeId,
                    };
                    await _context.APAROffsets.AddAsync(aparOffset);
                }

                await _context.SaveChangesAsync();
                await dbTransaction.CommitAsync();

                return requestId;
            }
            catch
            {
                await dbTransaction.RollbackAsync();
                throw;
            }
        }
    }
}
