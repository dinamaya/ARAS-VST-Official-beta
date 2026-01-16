using ARAS.Blazor.Models.DTOs;
using ARAS.Blazor.Repositories.Interfaces;
using ARAS.Blazor.Services.Interfaces;

namespace ARAS.Blazor.Services.Implementations
{
    public class BaseReceiptAdjustmentCommandService<TCreate, TRow> :
		ICreateReceiptRepository<TRow>
	{
		public BaseReceiptAdjustmentCommandService(
			IBaseReceiptAdjustmentService<TCreate> baseAdjustment,
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
		protected readonly IBaseReceiptAdjustmentService<TCreate> _baseAdjustment;
		protected string baseUrl { get; }
		protected string adjustmentTypeCode { get; }

		public async Task Create(TRow rows, IEnumerable<NoteRowDto> notes)
		{
			var requestsDto = mapperCallBack(rows);
			await _baseAdjustment.Create(requestsDto, notes, baseUrl);
		}

		public async Task Update(long requestId, TRow rows, IEnumerable<NoteRowDto> notes)
		{
			var requestsDto = mapperCallBack(rows);
			await _baseAdjustment.Update(requestId, requestsDto, notes, baseUrl);
		}
	}
}
