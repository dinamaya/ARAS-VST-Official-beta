using ARAS.Main.SSMS.Api.App_Code.Globals;
using ARAS.Main.SSMS.Api.App_Code.Globals.Constants;
using ARAS.Main.SSMS.Api.Context;
using ARAS.Main.SSMS.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.Models.SQLVIews;
using ARAS.Main.SSMS.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ARAS.Main.SSMS.Api.Repositories.Implementations
{
	public class SRAutoNetRepository :
		BaseAdjustmentCommandService<SRAutoNetRowDto, SRAutoNetCreateDto, object>,
		ISRAutoNetRepository
	{
		private readonly MainDbContext _context;
		public SRAutoNetRepository(
			IBaseAdjustmentRepository<SRAutoNetCreateDto> baseAdjustmentRepo,
			IAdjustmentRepository adjustmentRepo,
			IInvoiceRepository invoiceRepo,
			MainDbContext context) :
			base("srr", Map, baseAdjustmentRepo, adjustmentRepo, invoiceRepo)
		{
			_context = context;
		}

		public async Task<long> CreateAsync(RequestCreationDto<AdjustmentRequestCreationDto<SRAutoNetCreateDto>> data, string createdBy)
		{
			ArgumentNullException.ThrowIfNull(data, nameof(SRAutoNetCreateDto));

			return await _baseAdjustmentRepo.Create(data, createdBy, AdjustmentTypeCode, CreateInvoiceAdjustmentCallBack(createdBy));
		}

		public async Task<long> UpdateAsync(long requestId, RequestCreationDto<AdjustmentRequestCreationDto<SRAutoNetCreateDto>> data, string modifiedBy)
		{
			ArgumentNullException.ThrowIfNull(data, nameof(CashDiscountCreateDto));
			Guards.ThrowInvalidOperationIf(!data.Model.Adjustments.Any(), Exceptions.EMPTY_SRAUTONET_ROWS);

			return await _baseAdjustmentRepo.Update(requestId, data, modifiedBy, AdjustmentTypeCode, "sr-auto-net", CreateInvoiceAdjustmentCallBack(modifiedBy));
		}

		public Task<bool> IsValid(object inputValidation)
		{
			throw new NotImplementedException();
		}

		public override async Task<IEnumerable<SRAutoNetRowDto>> GetAdjustmentsByRequestId(long requestId)
		{
			var rows = (await base.GetAdjustmentsByRequestId(requestId)).ToList();
			var invoiceIds = rows
				.Select(r => r.InvoiceId)
				.Distinct()
				.ToList();

			var remarksLookup = await _context.CNDetails
				.AsNoTracking()
				.Where(c => invoiceIds.Contains(c.InvoiceId))
				.Select(c => new
				{
					c.InvoiceId,
					Remark = new SRAutoNetRemarksDto
					{
						CNRef = c.CNRef,
						CNAmt = c.CNAMT,
						WT = c.WT
					}
				})
				.ToListAsync();

			var groupedRemarks = remarksLookup
				.GroupBy(x => x.InvoiceId)
				.ToDictionary(
					g => g.Key,
					g => g.Select(x => x.Remark).ToList()
				);

			foreach (var row in rows)
				row.Remarks = groupedRemarks.TryGetValue(row.InvoiceId, out var remarks) ? remarks : [];

			return rows;
		}

		private static Func<RequestAdjustmentsV, SRAutoNetRowDto> Map = (requestAdjustment) =>
			new SRAutoNetRowDto()
			{
				Id = requestAdjustment.AdjustmentId.ToString(),
				InvoiceId = requestAdjustment.InvoiceId,
				AdjustmentAmount = requestAdjustment.AdjustmentAmount,
				AdjustmentActivity = requestAdjustment.AdjustmentActivity,
				InvoiceAmount = requestAdjustment.InvoiceAmount,
				InvoiceDate = requestAdjustment.InvoiceDate.ToString(Formats.Date.DISPLAY),
				InvoiceNumber = requestAdjustment.InvoiceNumber,
				CustomerName = requestAdjustment.CustomerName,
				CustomerNumber = requestAdjustment.CustomerNumber,
				ReasonCode = requestAdjustment.ReasonCode,
			};

		private Func<SRAutoNetCreateDto, string, long, Task> CreateInvoiceAdjustmentCallBack(string createdBy)
		{
			return (item, adjustmentTypeId, requestId) =>
			{
				return CreateInvoiceAdjustments(createdBy, requestId, adjustmentTypeId, item);
			};
		}

		private async Task CreateInvoiceAdjustments(string createdBy, long requestId, string adjustmentTypeId, SRAutoNetCreateDto data)
		{
			var sum = data.Remarks.Sum(r => r.WT);
			var invoice = new SRAutoNetInvoiceCreateDto();
			invoice.InvoiceAmount = data.InvoiceAmount;
			invoice.InvoiceNumber = data.InvoiceNumber;
			invoice.InvoiceDate = data.InvoiceDate;
			invoice.CustomerNumber = data.CustomerNumber;
			invoice.CustomerName = data.CustomerName;
			invoice.ReasonCode = data.ReasonCode;
			invoice.Remarks = data.Remarks;

			var invoiceId = await _invoiceRepo.CreateAsync(invoice, createdBy);

			var adjustment = new AdjustmentCreateDto(invoiceId, requestId, sum, adjustmentTypeId, 0, string.Empty, data.ReasonCode);
			await _adjustmentRepo.CreateAsync(adjustment, createdBy);
		}
	}
}
