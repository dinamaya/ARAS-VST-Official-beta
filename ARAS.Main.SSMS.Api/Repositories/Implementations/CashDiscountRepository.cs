using ARAS.Main.SSMS.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.App_Code.Globals;
using ARAS.Main.SSMS.Api.App_Code.Globals.Constants;
using ARAS.Main.SSMS.Api.Context;
using ARAS.Main.SSMS.Api.Repositories.Interfaces;
using ARAS.Main.SSMS.Api.Models.SQLVIews;

namespace ARAS.Main.SSMS.Api.Repositories.Implementations
{
	public class CashDiscountRepository : 
		BaseAdjustmentCommandService<CashDiscountRowDto, CashDiscountCreateDto, CashDiscountCreateValidationDto>,
		ICashDiscountRepository
	{
		public CashDiscountRepository(
			IBaseAdjustmentRepository<CashDiscountCreateDto> baseAdjustmentRepo, 
			IAdjustmentRepository adjustmentRepo, 
			IInvoiceRepository invoiceRepo) :
			base("cdr", Map, baseAdjustmentRepo, adjustmentRepo, invoiceRepo)
		{
		}

		public async Task<long> CreateAsync(RequestCreationDto<AdjustmentRequestCreationDto<CashDiscountCreateDto>> data, string createdBy)
		{
			ArgumentNullException.ThrowIfNull(data, nameof(CashDiscountCreateDto));

			return await _baseAdjustmentRepo.Create(data, createdBy, AdjustmentTypeCode, CreateInvoiceAdjustmentCallBack(createdBy));
		}

		public async Task<long> UpdateAsync(long requestId, RequestCreationDto<AdjustmentRequestCreationDto<CashDiscountCreateDto>> data, string modifiedBy)
		{
			ArgumentNullException.ThrowIfNull(data, nameof(CashDiscountCreateDto));
			Guards.ThrowInvalidOperationIf(!data.Model.Adjustments.Any(), Exceptions.EMPTY_CASHDISCOUNT_ROWS);

			return await _baseAdjustmentRepo.Update(requestId, data, modifiedBy, AdjustmentTypeCode, "cash-discount", CreateInvoiceAdjustmentCallBack(modifiedBy));
		}

		public async Task<bool> IsValid(CashDiscountCreateValidationDto cashCreateValidationRequest)
		{
			return await _invoiceRepo.IsInvoiceNumberAvailable(cashCreateValidationRequest.InvoiceNumber, "CDR");
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

		private Func<CashDiscountCreateDto, string, long, Task> CreateInvoiceAdjustmentCallBack(string createdBy)
		{
			return (item, adjustmentTypeId, requestId) =>
			{
				return CreateInvoiceAdjustments(createdBy, requestId, adjustmentTypeId, item);
			};
		}

		private async Task CreateInvoiceAdjustments(string createdBy, long requestId, string adjustmentTypeId, CashDiscountCreateDto data)
		{
			var invoice = new InvoiceCreateDto(data.InvoiceNumber, data.InvoiceAmount, data.InvoiceDate, data.CustomerNumber, data.CustomerName);
			var invoiceId = await _invoiceRepo.CreateAsync(invoice, createdBy);

			double adjustmentAmount = data.DiscountValue * data.InvoiceAmount;
			float discountPercent = data.DiscountValue * 100;
			var adjustment = new AdjustmentCreateDto(invoiceId, requestId, adjustmentAmount, adjustmentTypeId, discountPercent, data.Remarks, data.ReasonCode);
			await _adjustmentRepo.CreateAsync(adjustment, createdBy);
		}
	}
}
