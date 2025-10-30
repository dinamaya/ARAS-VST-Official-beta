using ARAS.Blazor.Models.DTOs;

namespace ARAS.Blazor.Services.Interfaces
{
	public interface IBaseService
	{
		Task<ResponseDto<TResult>> SendAsync<TResult>(RequestDto requestDto,
			bool withBearer = true,
			Func<RequestDto, Task>? onBeforeSendCallBack = null,
			Func<ResponseDto<TResult>, Task>? onSuccessSendCallBack = null,
			Func<RequestDto, Task>? onErrorSendCallBack = null
		);
	}
}
