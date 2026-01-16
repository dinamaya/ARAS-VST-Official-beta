using ARAS.Main.SSMS.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.Repositories.Interfaces;

namespace ARAS.Main.SSMS.Api.Repositories.Implementations
{
    public class BaseReceiptAdjustmentCommandRepository<TCreate> :
		ICreateReceiptStatusRepository<TCreate>
            where TCreate : BaseAdjustmentCreateDto
    {
		protected readonly IBaseReceiptAdjustmentRepository<TCreate> _baseAdjustmentRepo;
		protected string AdjustmentTypeCode { get; }

		public BaseReceiptAdjustmentCommandRepository(
			string adjustmentTypeCode,
			IBaseReceiptAdjustmentRepository<TCreate> baseAdjustmentRepo,
			IAdjustmentRepository adjustmentRepo,
			IInvoiceRepository invoiceRepo)
		{
			AdjustmentTypeCode = adjustmentTypeCode;

			_baseAdjustmentRepo = baseAdjustmentRepo;
		}

        public async Task<long> CreateAsync(RequestCreationDto<TCreate> data, string createdBy) => await _baseAdjustmentRepo.Create(data, createdBy, AdjustmentTypeCode);

        public async Task<long> UpdateAsync(long requestId, RequestCreationDto<TCreate> data, string modifiedBy) => await _baseAdjustmentRepo.Update(requestId, data, modifiedBy, AdjustmentTypeCode);
    }
}
