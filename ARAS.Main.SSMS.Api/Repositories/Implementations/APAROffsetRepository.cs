using ARAS.Main.SSMS.Api.Context;
using ARAS.Main.SSMS.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.Models.Entities;
using ARAS.Main.SSMS.Api.Repositories.Interfaces;
using ARAS.Main.SSMS.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ARAS.Main.SSMS.Api.Repositories.Implementations
{
    public class APAROffsetRepository : IAPAROffsetRepository
    {
        private readonly MainDbContext _context;
        private readonly IRequestRepository _requestRepo;
        private readonly ITransactionRepository _transactionRepo;
        private readonly IBackgroundJobService _bgJobService;
        private readonly IAdjustmentRepository _adjustmentRepo;
        private readonly IInvoiceRepository _invoiceRepo;

        public APAROffsetRepository(MainDbContext context, IRequestRepository requestRepo, ITransactionRepository transactionRepo, IBackgroundJobService bgJobService, IAdjustmentRepository adjustmentRepo, IInvoiceRepository invoiceRepo)
        {
            _context = context;
            _requestRepo = requestRepo;
            _transactionRepo = transactionRepo;
            _bgJobService = bgJobService;
            _adjustmentRepo = adjustmentRepo;
            _invoiceRepo = invoiceRepo;
        }

        public async Task<long> CreateAsync(RequestCreationDto<AdjustmentRequestCreationDto<APAROffsetCreateDto>> data, string createdBy)
        {
            await using var dbTransaction = await _context.Database.BeginTransactionAsync();

            try
            {
                string aparOffsetTypeId = await _context.AdjustmentTypes.Where(x => x.Code.Equals("ARR")).Select(x => x.Id).FirstAsync();
                string referenceNo = "APAR-" + DateTime.Now.Ticks;

                var request = new RequestCreateDto(referenceNo, aparOffsetTypeId);
                var requestId = await _requestRepo.CreateAsync(request, createdBy);

                var transaction = new TransactionCreateDto(requestId, "Pending");
                var transactId = await _transactionRepo.CreateAsync(transaction, createdBy);

                foreach (var item in data.Model.Adjustments)
                    await CreateInvoiceAdjustments(createdBy, requestId, aparOffsetTypeId, item);

                //var timeline = await _transactionRepo.GetEmailHistoryByRequestId(requestId);

                await _bgJobService.RunSendRequestPending(new ProceedEmailDto
                {
                    RequestId = requestId.ToString(),
                    RequestorName = data.CreatorFullName,
                    AdjustmentType = "APAR Offset",
                    RequestNumber = referenceNo,
                    Status = "Pending",
                    Timeline = [],
                    ToEmail = data.Model.ToEmail,
                });

                await dbTransaction.CommitAsync();

                return requestId;
            }
            catch
            {
                await dbTransaction.RollbackAsync();
                throw;
            }
        }

        private async Task CreateInvoiceAdjustments(string createdBy, long requestId, string adjustmentTypeId, APAROffsetCreateDto data)
        {
            var invoice = new InvoiceCreateDto(data.InvoiceId, data.Amount, invoiceDate: DateTime.Now, data.CustomerNumber, data.CustomerName);
            var invoiceId = await _invoiceRepo.CreateAsync(invoice, createdBy);

            var aparOffset = new APAROffset
            {
                RequestId = requestId,
                InvoiceId = invoiceId,
                Amount = data.Amount,
                Type = data.Type,
                AdjustmentTypeId = adjustmentTypeId,

                CreatedBy = createdBy,
                DateCreated = DateTime.Now,
                ModifiedBy = createdBy,
                DateModified = DateTime.Now,
                IsActive = true
            };
            
            await _context.APAROffsets.AddAsync(aparOffset);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsValid(CashDiscountCreateValidationDto cashCreateValidationRequest)
        {
            return await _invoiceRepo.IsInvoiceNumberAvailable(cashCreateValidationRequest.InvoiceNumber, "CDR");
        }
    }
}
