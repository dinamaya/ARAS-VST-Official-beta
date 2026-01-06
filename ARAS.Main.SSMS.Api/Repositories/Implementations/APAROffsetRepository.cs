using ARAS.Main.SSMS.Api.App_Code.Globals;
using ARAS.Main.SSMS.Api.App_Code.Globals.Constants;
using ARAS.Main.SSMS.Api.Context;
using ARAS.Main.SSMS.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.Models.SQLVIews;
using ARAS.Main.SSMS.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace ARAS.Main.SSMS.Api.Repositories.Implementations
{
    public class APAROffsetRepository :
		BaseAdjustmentCommandService<object, APAROffsetCreateDto, object>,
		IAPAROffsetRepository
	{
		private readonly MainDbContext _context;
		public APAROffsetRepository(
			IBaseAdjustmentRepository<APAROffsetCreateDto> baseAdjustmentRepo,
			IAdjustmentRepository adjustmentRepo,
			IInvoiceRepository invoiceRepo,
			MainDbContext context) :
			base("arr", Map, baseAdjustmentRepo, adjustmentRepo, invoiceRepo)
		{
			_context = context;
		}

		public async Task<long> CreateAsync(RequestCreationDto<AdjustmentRequestCreationDto<APAROffsetCreateDto>> data, string createdBy)
		{
			ArgumentNullException.ThrowIfNull(data, nameof(CashDiscountCreateDto));
			
			return await _baseAdjustmentRepo.Create(data, createdBy, AdjustmentTypeCode, CreateInvoiceAdjustmentCallBack(createdBy));
		}

		public async Task<long> UpdateAsync(long requestId, RequestCreationDto<AdjustmentRequestCreationDto<APAROffsetCreateDto>> data, string modifiedBy)
		{
			ArgumentNullException.ThrowIfNull(data, nameof(CashDiscountCreateDto));
			Guards.ThrowInvalidOperationIf(!data.Model.Adjustments.Any(), Exceptions.EMPTY_CASHDISCOUNT_ROWS);

			return await _baseAdjustmentRepo.Update(requestId, data, modifiedBy, AdjustmentTypeCode, "cash-discount", CreateInvoiceAdjustmentCallBack(modifiedBy));
		}

		public Task<bool> IsValid(object inputValidation)
		{
			throw new NotImplementedException();
		}

		private Func<APAROffsetCreateDto, string, long, Task> CreateInvoiceAdjustmentCallBack(string createdBy)
		{
			return (item, adjustmentTypeId, requestId) =>
			{
				return CreateInvoiceAdjustments(createdBy, requestId, adjustmentTypeId, item);
			};
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
						  : ARRowType.Invoice
					 })
					.ToListAsync()
			};
		}

		private async Task CreateInvoiceAdjustments(string createdBy, long requestId, string adjustmentTypeId, APAROffsetCreateDto data)
		{
			var invoice = new InvoiceCreateDto(data.InvoiceId, data.Amount, invoiceDate: DateTime.Now, data.CustomerNumber, data.CustomerName);
			var invoiceId = await _invoiceRepo.CreateAsync(invoice, createdBy);

			await _adjustmentRepo.CreateAPAROffsetAdjustmentAsync(data, invoiceId, createdBy, requestId, adjustmentTypeId);
		}

		private static Func<RequestAdjustmentsV, CashDiscountRowDto> Map = (requestAdjustment) =>
	        new CashDiscountRowDto()
	        {
		        Id = requestAdjustment.AdjustmentId.ToString(),
		        DiscountValue = requestAdjustment.DiscountPercentage / 100,
		        AdjustmentAmount = requestAdjustment.AdjustmentAmount,
		        AdjustmentActivity = requestAdjustment.AdjustmentActivity,
		        InvoiceAmount = requestAdjustment.InvoiceAmount,
		        InvoiceDate = requestAdjustment.InvoiceDate.ToString(Formats.Date.DISPLAY),
		        InvoiceNumber = requestAdjustment.InvoiceNumber,
		        CustomerName = requestAdjustment.CustomerName,
		        CustomerNumber = requestAdjustment.CustomerNumber,
		        ReasonCode = requestAdjustment.ReasonCode,
		        Remarks = requestAdjustment.Remarks,
	        };
	}
}
