using ARAS.Main.SSMS.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.Models.SQLVIews;
using ARAS.Main.SSMS.Api.Repositories.Interfaces;

namespace ARAS.Main.SSMS.Api.Repositories.Implementations
{
	public class BaseAdjustmentCommandService<TRow, TCreate, TValidation>: 
		ICreateStatusRepository,
		IAdjustmentReaderRepository<TRow>
	{
		protected readonly IBaseAdjustmentRepository<TCreate> _baseAdjustmentRepo;
		protected readonly IAdjustmentRepository _adjustmentRepo;
		protected readonly IInvoiceRepository _invoiceRepo;

		public BaseAdjustmentCommandService(
			string adjustmentTypeCode,
			Func<RequestAdjustmentsV, TRow> mapperCallBack,
			IBaseAdjustmentRepository<TCreate> baseAdjustmentRepo,
			IAdjustmentRepository adjustmentRepo,
			IInvoiceRepository invoiceRepo)
		{
			AdjustmentTypeCode = adjustmentTypeCode;
			MapperCallBack = mapperCallBack;

			_baseAdjustmentRepo = baseAdjustmentRepo;
			_adjustmentRepo = adjustmentRepo;
			_invoiceRepo = invoiceRepo;
		}

		public string AdjustmentTypeCode { get; }
		public Func<RequestAdjustmentsV, TRow> MapperCallBack { get; }

		public async Task ApproveAsync(RequestUpdateDto data, string createdBy) => await _baseAdjustmentRepo.Approve(data, createdBy, AdjustmentTypeCode);
		public async Task ValidateAsync(RequestUpdateDto data, string createdBy) => await _baseAdjustmentRepo.Validate(data, createdBy, AdjustmentTypeCode);
		public async Task DeclineAsync(NegateRequestDto data, string createdBy) => await _baseAdjustmentRepo.Decline(data, createdBy, AdjustmentTypeCode);
		public async Task RejectAsync(NegateRequestDto data, string createdBy) => await _baseAdjustmentRepo.Reject(data, createdBy, AdjustmentTypeCode);
		public virtual async Task<IEnumerable<TRow>> GetAdjustmentsByRequestId(long requestId) =>
			(await _adjustmentRepo.GetAllByRequestIdAndCode(requestId, AdjustmentTypeCode)).Select(MapperCallBack);
	}
}
