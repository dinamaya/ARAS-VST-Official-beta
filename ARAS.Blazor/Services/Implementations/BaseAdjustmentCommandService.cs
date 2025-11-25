using ARAS.Blazor.Models.DTOs;
using ARAS.Blazor.Repositories.Interfaces;
using ARAS.Blazor.Services.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ARAS.Blazor.Services.Implementations
{
	public class BaseAdjustmentCommandService<TCreate, TRow, TValidation> :
		ICreateStatusRepository<TRow>,
		IAdjustmentReaderRepository<TRow>,
		IRequestSubmissionReaderRepository,
		IInputValidatorRepository<TValidation>
	{
		protected readonly IBaseAdjustmentService<TCreate, TRow, TValidation> _baseAdjustment;
		protected string baseUrl { get; }
		protected string adjustmentTypeCode { get; }

		public BaseAdjustmentCommandService(
			IBaseAdjustmentService<TCreate, TRow, TValidation> baseAdjustment, 
			string baseUrl, 
			string adjustmentTypeCode, 
			Func<TRow, TCreate> mapperCallBack)
		{
			_baseAdjustment = baseAdjustment;
			this.baseUrl = baseUrl;
			this.adjustmentTypeCode = adjustmentTypeCode;
			this.mapperCallBack = mapperCallBack;
		}

		private Func<TRow, TCreate> mapperCallBack { get; }

		public async Task<IEnumerable<TRow>> GetAdjustments(long requestId) => await _baseAdjustment.GetAdjustmentsByRequestIdAndTypeCode(requestId, adjustmentTypeCode);
	
		public async Task Approve(long requestId, IEnumerable<NoteRowDto> notes) => await _baseAdjustment.Approve(requestId, notes, adjustmentTypeCode);

		public async Task Decline(NegateRequestDto createDecline, IEnumerable<NoteRowDto> notes) => await _baseAdjustment.Decline(createDecline, notes, adjustmentTypeCode);

		public async Task Reject(NegateRequestDto createReject, IEnumerable<NoteRowDto> notes) => await _baseAdjustment.Reject(createReject, notes, adjustmentTypeCode);

		public async Task Validate(long requestId, IEnumerable<NoteRowDto> notes) => await _baseAdjustment.Validate(requestId, notes, adjustmentTypeCode);

		public async Task Create(IEnumerable<TRow> rows, IEnumerable<NoteRowDto> notes)
		{
			var requestsDto = rows.Select(mapperCallBack).ToList();
			await _baseAdjustment.Create(requestsDto, notes, baseUrl);
		}

		public async Task Update(long requestId, IEnumerable<TRow> rows, IEnumerable<NoteRowDto> notes)
		{
			var requestsDto = rows.Select(mapperCallBack).ToList();
			await _baseAdjustment.Update(requestId, requestsDto, notes, baseUrl);
		}

		public async Task<IEnumerable<TransactionRequestRowDto>> GetSubmissions() => await _baseAdjustment.GetSubmissions(adjustmentTypeCode);
		public async Task<IEnumerable<TransactionRequestRowDto>> GetApprovals() => await _baseAdjustment.GetApprovals(adjustmentTypeCode);
		public async Task<IEnumerable<TransactionRequestRowDto>> GetValidations() => await _baseAdjustment.GetValidations(adjustmentTypeCode);

		public async Task<bool> IsValid(TValidation inputValidation) => await _baseAdjustment.IsValid(adjustmentTypeCode, inputValidation);
	}
}
