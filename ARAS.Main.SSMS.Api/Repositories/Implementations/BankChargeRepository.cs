using ARAS.Main.SSMS.Api.App_Code.Globals;
using ARAS.Main.SSMS.Api.App_Code.Globals.Constants;
using ARAS.Main.SSMS.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.Models.SQLVIews;
using ARAS.Main.SSMS.Api.Repositories.Interfaces;

namespace ARAS.Main.SSMS.Api.Repositories.Implementations
{
	public class BankChargeRepository : 
		BaseAdjustmentCommandService<BankChargeRowDto, BankChargeCreateDto, BaseaAdjustmentCreateValidationDto>,
		IBankChargeRepository
	{
		public BankChargeRepository(
			IBaseAdjustmentRepository<BankChargeCreateDto> baseAdjustmentRepo, 
			IAdjustmentRepository adjustmentRepo, 
			IInvoiceRepository invoiceRepo) :
			base("bca", Map, baseAdjustmentRepo, adjustmentRepo, invoiceRepo)
		{
		}
		
		public async Task<long> CreateAsync(RequestCreationDto<AdjustmentRequestCreationDto<BankChargeCreateDto>> data, string createdBy)
		{
			ArgumentNullException.ThrowIfNull(data, nameof(BankChargeCreateDto));

			return await _baseAdjustmentRepo.Create(data, createdBy, AdjustmentTypeCode, CreateInvoiceAdjustmentCallBack(createdBy));
		}

		public async Task<long> UpdateAsync(long requestId, RequestCreationDto<AdjustmentRequestCreationDto<BankChargeCreateDto>> data, string modifiedBy)
		{
			ArgumentNullException.ThrowIfNull(data, nameof(BankChargeCreateDto));
			Guards.ThrowInvalidOperationIf(!data.Model.Adjustments.Any(), Exceptions.EMPTY_CASHDISCOUNT_ROWS);

			return await _baseAdjustmentRepo.Update(requestId, data, modifiedBy, AdjustmentTypeCode, "bank-charge", CreateInvoiceAdjustmentCallBack(modifiedBy));
		}



		private static Func<RequestAdjustmentsV, BankChargeRowDto> Map = (requestAdjustment) =>
			new BankChargeRowDto()
			{
				Id = requestAdjustment.AdjustmentId.ToString(),
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

		private Func<BankChargeCreateDto, string, long, Task> CreateInvoiceAdjustmentCallBack(string createdBy)
		{
			return (item, adjustmentTypeId, requestId) =>
			{
				return CreateInvoiceAdjustments(createdBy, requestId, adjustmentTypeId, item);
			};
		}

		private async Task CreateInvoiceAdjustments(string createdBy, long requestId, string adjustmentTypeId, BankChargeCreateDto data)
		{
			var invoice = new InvoiceCreateDto(data.InvoiceNumber, data.InvoiceAmount, data.InvoiceDate, data.CustomerNumber, data.CustomerName);
			var invoiceId = await _invoiceRepo.CreateAsync(invoice, createdBy);

			var adjustment = new AdjustmentCreateDto(invoiceId, requestId, data.AdjustmentAmount, adjustmentTypeId, 0, data.Remarks, data.ReasonCode);
			await _adjustmentRepo.CreateAsync(adjustment, createdBy);
		}

		public async Task<bool> IsValid(BaseaAdjustmentCreateValidationDto inputValidation)
		{
			return await _invoiceRepo.IsInvoiceNumberAvailable("", "BCA");
		}
	}
}
