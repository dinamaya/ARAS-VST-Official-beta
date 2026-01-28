using ARAS.Main.SSMS.Api.Context;
using ARAS.Main.SSMS.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.Models.Entities;
using ARAS.Main.SSMS.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ARAS.Main.SSMS.Api.Repositories.Implementations
{
    public class APAROffsetRepository : IAPAROffsetRepository
    {
		private readonly MainDbContext _context;
		private readonly IAdjustmentRepository _adjustmentRepo;
		private readonly IRequestRepository _requestRepo;
		private readonly ITransactionRepository _transactionRepo;
		private readonly IInvoiceRepository _invoiceRepo;

        public APAROffsetRepository(IAdjustmentRepository adjustmentRepo, IRequestRepository requestRepo, ITransactionRepository transactionRepo, IInvoiceRepository invoiceRepo, MainDbContext context)
        {
            _adjustmentRepo = adjustmentRepo;
            _requestRepo = requestRepo;
            _transactionRepo = transactionRepo;
            _invoiceRepo = invoiceRepo;
            _context = context;
        }

        public async Task<long> Create(RequestCreationDto<IEnumerable<APAROffsetCreateDto>> data, string createdBy)
        {
			await using var dbTransaction = await _context.Database.BeginTransactionAsync();

			try
			{
				IList<InvoiceCreateDto> invoices = [];

				IList<ReasonAdjustmentCreateDto> reasons = [];
				IEnumerable<long> invoiceIds = [];
				IEnumerable<long> reasonInvoiceIds = [];

				IList<APAROffsetCreateDto> adjustments = [];

				string adjustmentTypeId = (await _adjustmentRepo.GetAdjustmentInfoByCode("ARR")).Id;
				var requestId = await _requestRepo.CreateAsync(adjustmentTypeId, createdBy);

				var transaction = new TransactionCreateDto(requestId, "Pending");
				var transactId = await _transactionRepo.CreateAsync(transaction, createdBy);

				foreach (var _data in data.Model)
				{
					if (!string.IsNullOrEmpty(_data.ReasonCode))
						reasons.Add(new ReasonAdjustmentCreateDto(requestId, _data.ReasonCode, _data.Amount, _data.CustomerNumber, _data.CustomerName));
					else
						invoices.Add(new InvoiceCreateDto(_data.InvoiceNumber, _data.Amount, invoiceDate: DateTime.Now, _data.CustomerNumber, _data.CustomerName));
				}

				invoiceIds = await _invoiceRepo.CreateAsync(invoices, createdBy);
				reasonInvoiceIds = await _invoiceRepo.CreateAsync(reasons, createdBy);

				// Combine the model, invoiceIds and requestIds
				var zippedInvoices = invoiceIds
					.Zip(data.Model, (inv, mod) => new
					{
						invoice = inv,
						model = mod
					});
				var zippedReasons = reasonInvoiceIds
					.Zip(reasons, (inv, mod) => new
					{
						invoice = inv,
						model = mod
					});

				foreach (var result in zippedInvoices)
					adjustments.Add(new APAROffsetCreateDto()
					{
						RequestId = requestId,
						InvoiceNumber = result.invoice.ToString(),
						Amount = result.model.Amount,
						Type = result.model.Type,
						ReasonCode = result.model.ReasonCode ?? string.Empty,
						InvoiceDate = result.model.InvoiceDate,
					});


				foreach (var result in zippedReasons)
					adjustments.Add(new APAROffsetCreateDto()
					{
						RequestId = requestId,
						InvoiceNumber = result.invoice.ToString(),
						Amount = result.model.Amount,
						Type = "AR",
						ReasonCode = result.model.ReasonCode ?? string.Empty,
						InvoiceDate = DateTime.MinValue,
					});

				await _adjustmentRepo.CreateAsync(adjustments, createdBy);
				await dbTransaction.CommitAsync();

				return requestId;
			}
			catch
			{
				await dbTransaction.RollbackAsync();
				throw;
			}
		}

        public async Task<long> Update(long requestId, RequestCreationDto<IEnumerable<APAROffsetCreateDto>> data, string modifiedBy)
        {
            throw new NotImplementedException();
        }

		public async Task<APAROffsetRowDto> GetAPAdjustmentsByRequestId(long requestId)
		{
			return new APAROffsetRowDto()
			{
				APGroup = await _context.VwAparoffsetRows.AsNoTracking()
					.Where(r => r.Type == "AP" && r.RequestiD == requestId && r.IsActive)
					.Select(r => new APAROffsetAPRowDto()
					{
						Id = r.Id.ToString(),
						InvoiceAmount = r.InvoiceAmount,
						InvoiceNumber = r.InvoiceNumber,
						InvoiceDate = r.InvoiceDate,
						CustomerName = r.CustomerName,
						CustomerNumber = r.CustomerNumber,
					})
				.ToListAsync(),
				ARGroup = await _context.VwAparoffsetRows.AsNoTracking()
					 .Where(r => r.Type == "AR" && r.RequestiD == requestId && r.IsActive)
					 .Select(r => new APAROffsetARRowDto()
					 {
						 Id = r.Id.ToString(),
						 Amount = r.InvoiceAmount,
						 InvoiceNumber = r.InvoiceNumber,
						 AdjustmentReason = r.ReasonCode,
						 RowType = !string.IsNullOrEmpty(r.ReasonCode)
						   ? ARRowType.Adjustment
						  : ARRowType.Invoice,

						 CustomerName = r.CustomerName,
						 CustomerNumber = r.CustomerNumber
					 })
					.ToListAsync()
			};
		}
	}
}
