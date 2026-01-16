using ARAS.Blazor.App_Code.Globals;
using ARAS.Blazor.App_Code.Globals.Enums;
using ARAS.Blazor.Models.DTOs;
using ARAS.Blazor.Services.Interfaces;

namespace ARAS.Blazor.Services.Implementations
{
    public class BaseReceiptAdjustmentService<TCreate> : IBaseReceiptAdjustmentService<TCreate>
    {
		private readonly IBaseService _baseService;
		private readonly INoteService _noteService;

        public BaseReceiptAdjustmentService(IBaseService baseService, INoteService noteService)
        {
            _baseService = baseService;
            _noteService = noteService;
        }

        public async Task Create(TCreate data, IEnumerable<NoteRowDto> notes, string route)
        {

			var createResult = await _baseService.SendAsync<long>(new RequestDto<TCreate>()
			{
				ApiType = ApiType.POST,
				URL = route,
				Data = data
			},
				onSuccessSendCallBack: (resp) =>
				{
					Guards.ThrowInvalidOperationIf(!resp.IsSuccess, "Failed to create request" + resp.Message);
					return Task.CompletedTask;
				}
			);

			await _noteService.Create(createResult.Result, notes);
		}

        public async Task Update(long requestId, TCreate data, IEnumerable<NoteRowDto> notes, string route)
        {
			await _baseService.SendAsync<string>(
				new RequestDto<TCreate>()
				{
					ApiType = ApiType.POST,
					URL = $"{route}/{requestId}",
					Data = data
				},
				onSuccessSendCallBack: (resp) =>
				{
					Guards.ThrowInvalidOperationIf(!resp.IsSuccess, "Failed to update request" + resp.Message);
					return Task.CompletedTask;
				}
			);

			await _noteService.Create(requestId, notes);
		}
    }
}
