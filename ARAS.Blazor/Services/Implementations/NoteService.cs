using ARAS.Blazor.App_Code.Globals;
using ARAS.Blazor.App_Code.Globals.Enums;
using ARAS.Blazor.Models.DTOs;
using ARAS.Blazor.Services.Interfaces;

namespace ARAS.Blazor.Services.Implementations
{
	public class NoteService : INoteService
	{
		private readonly IBaseService _baseService;
		private readonly IConfigService _configService;

		public NoteService(IBaseService baseService, IConfigService configService)
		{
			_baseService = baseService;
			_configService = configService;
		}

		public async Task Create(long requestId, IEnumerable<NoteRowDto> notes)
		{

			var filtered = notes.Where(n => string.IsNullOrEmpty(n.Id));
			if (!filtered.Any()) return;
			Guards.ThrowInvalidOperationIf(requestId == default || requestId == 0, "There is a problem while creating notes");

			await _baseService.SendAsync<string>(
				new RequestDto()
				{
					ApiType = ApiType.POST,
					URL = _configService.GetFilesUrl($"notes/upload/{requestId}"),
					Data = filtered,
					ContentType = ContentType.MultipartFormData,
					FormCollectionName = "notes"
				},
				onSuccessSendCallBack: async (resp) =>
				{
					await Task.Run(() =>
					{
						Guards.ThrowInvalidOperationIf(!resp.IsSuccess, resp.Message);
					});
				});
		}

		public async Task Create(IEnumerable<NoteRowDto> notes)
		{

			await _baseService.SendAsync<string>(
				new RequestDto()
				{
					ApiType = ApiType.POST,
					URL = _configService.GetFilesUrl($"notes"),
					Data = notes,
					ContentType = ContentType.MultipartFormData,
					FormCollectionName = "notes"
				},
				onSuccessSendCallBack: async (resp) =>
				{
					await Task.Run(() =>
					{
						Guards.ThrowInvalidOperationIf(!resp.IsSuccess, resp.Message);
					});
				});
		}

		public async Task<IList<NoteRowDto>> GetRows(long requestId)
		{
			var response = await _baseService.SendAsync<IEnumerable<NoteRowDto>>(
				new RequestDto()
				{
					URL = _configService.GetFilesUrl($"notes/{requestId}"),
				},
				onSuccessSendCallBack: async (resp) =>
				{
					await Task.Run(() =>
					{
						Guards.ThrowInvalidOperationIf(!resp.IsSuccess, resp.Message);
					});
				});

			return response.Result.ToList();
		}
	}
}
