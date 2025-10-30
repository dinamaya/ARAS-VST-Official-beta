using ARAS.Blazor.App_Code.Globals;
using ARAS.Blazor.Models.DTOs;
using ARAS.Blazor.Services.Interfaces;
using ARAS.Main.SSMS.Api.Models.Dtos;
using Azure;
using Microsoft.JSInterop;
using Radzen;

namespace ARAS.Blazor.Services.Implementations
{
	public class AttachmentService : IAttachmentService
	{

		private readonly IBaseService _baseService;
		private readonly IConfigService _configService;
		private readonly IJSRuntime _jsRuntime;

		public AttachmentService(IBaseService baseService, IConfigService configService, IJSRuntime jsRuntime)
		{
			_baseService = baseService;
			_configService = configService;
			_jsRuntime = jsRuntime;
		}

		public async Task Download(string noteId, string attachmentName)
		{
			var response = await _baseService.SendAsync<byte[]>(
				new RequestDto()
				{
					URL = _configService.GetFilesUrl($"notes/download/{noteId}"),
				},
				onSuccessSendCallBack: async (resp) =>
				{
					await Task.Run(() =>
					{
						Guards.ThrowInvalidOperationIf(!resp.IsSuccess, "Failed to download attachment: " + resp.Message);
						Guards.ThrowNullReferenceIf(resp.Result, "Failed to download attachment: " + resp.Message);
					});
				});

			if (response.Result == null) return;
			var base64Data = Convert.ToBase64String(response.Result);
			await _jsRuntime.InvokeVoidAsync("downloadFile", attachmentName, base64Data);
		}

		public async Task<FileUploadRequirementsDto> GetFileUploadRequirements()
		{
			var response = await _baseService.SendAsync<FileUploadRequirementsDto>(
				new RequestDto()
				{
					URL = _configService.GetFilesUrl("requirements"),
				},
				onSuccessSendCallBack: async (resp) =>
				{
				await Task.Run(() =>
				{
					Guards.ThrowInvalidOperationIf(!resp.IsSuccess, "Failed to fetch file upload requirements");
				});
			});

			return response.Result;
		}
	}
}
