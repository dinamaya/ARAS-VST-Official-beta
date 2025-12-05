using ARAS.Main.SSMS.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.App_Code.Globals;
using ARAS.Main.SSMS.Api.App_Code.Globals.Constants;
using ARAS.Main.SSMS.Api.Context;
using ARAS.Main.SSMS.Api.Repositories.Interfaces;
using ARAS.Main.SSMS.Api.Models.SQLVIews;

namespace ARAS.Main.SSMS.Api.Repositories.Implementations
{
    public class ARInvoiceOffsettingRepository :
        BaseAdjustmentCommandService<ARInvoiceOffsettingRowDto, ARInvoiceOffsettingCreateDto, ARInvoiceOffsettingCreateValidationDto>,
        IARInvoiceOffsettingRepository
    {
        public ARInvoiceOffsettingRepository(
            IBaseAdjustmentRepository<ARInvoiceOffsettingCreateDto> baseAdjustmentRepo,
            IAdjustmentRepository adjustmentRepo,
            IInvoiceRepository invoiceRepo) :
            base("aro", Map, baseAdjustmentRepo, adjustmentRepo, invoiceRepo)
        {
        }

        public async Task<long> CreateAsync(RequestCreationDto<AdjustmentRequestCreationDto<ARInvoiceOffsettingCreateDto>> data, string createdBy)
        {
            ArgumentNullException.ThrowIfNull(data, nameof(ARInvoiceOffsettingCreateDto));

            return await _baseAdjustmentRepo.Create(data, createdBy, AdjustmentTypeCode, CreateInvoiceAdjustmentCallBack(createdBy));
        }

        public async Task<long> UpdateAsync(long requestId, RequestCreationDto<AdjustmentRequestCreationDto<ARInvoiceOffsettingCreateDto>> data, string modifiedBy)
        {
            ArgumentNullException.ThrowIfNull(data, nameof(ARInvoiceOffsettingCreateDto));
            Guards.ThrowInvalidOperationIf(!data.Model.Adjustments.Any(), Exceptions.EMPTY_ARINVOICEOFFSETTING_ROWS);

            return await _baseAdjustmentRepo.Update(requestId, data, modifiedBy, AdjustmentTypeCode, "ar-invoice-offsetting", CreateInvoiceAdjustmentCallBack(modifiedBy));
        }

        public async Task<bool> IsValid(ARInvoiceOffsettingCreateValidationDto cashCreateValidationRequest)
        {
            return await _invoiceRepo.IsInvoiceNumberAvailable(cashCreateValidationRequest.InvoiceNumber, "ARO");
        }

        private static Func<RequestAdjustmentsV, ARInvoiceOffsettingRowDto> Map = (requestAdjustment) =>
            new ARInvoiceOffsettingRowDto()
            {
                Id = requestAdjustment.AdjustmentId.ToString(),
                AdjustmentActivity = requestAdjustment.AdjustmentActivity,
                InvoiceAmount = requestAdjustment.InvoiceAmount,
                InvoiceDate = requestAdjustment.InvoiceDate.ToString(Formats.Date.DISPLAY),
                InvoiceNumber = requestAdjustment.InvoiceNumber,
                CustomerName = requestAdjustment.CustomerName,
                CustomerNumber = requestAdjustment.CustomerNumber,
                Remarks = requestAdjustment.Remarks,
            };

        private Func<ARInvoiceOffsettingCreateDto, string, long, Task> CreateInvoiceAdjustmentCallBack(string createdBy)
        {
            return (item, adjustmentTypeId, requestId) =>
            {
                return CreateInvoiceAdjustments(createdBy, requestId, adjustmentTypeId, item);
            };
        }

        private async Task CreateInvoiceAdjustments(string createdBy, long requestId, string adjustmentTypeId, ARInvoiceOffsettingCreateDto data)
        {
            var invoice = new InvoiceCreateDto(data.InvoiceNumber, data.InvoiceAmount, data.InvoiceDate, data.CustomerNumber, data.CustomerName);
            var invoiceId = await _invoiceRepo.CreateAsync(invoice, createdBy);

            var adjustment = new AdjustmentCreateDto(invoiceId, requestId, data.InvoiceAmount, adjustmentTypeId, 0, data.Remarks, null);
            await _adjustmentRepo.CreateAsync(adjustment, createdBy);
        }
    }
}
