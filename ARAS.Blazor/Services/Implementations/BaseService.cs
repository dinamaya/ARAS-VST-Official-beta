using ARAS.Blazor.App_Code.Globals;
using ARAS.Blazor.App_Code.Globals.Constants;
using ARAS.Blazor.App_Code.Globals.Enums;
using ARAS.Blazor.Models.DTOs;
using ARAS.Blazor.Services.Interfaces;
using Azure;
using Newtonsoft.Json;
using Radzen;
using System.Net;
using System.Text;

namespace ARAS.Blazor.Services.Implementations
{
	public class BaseService : IBaseService
	{
		private readonly ITokenService _tokenService;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly ILogger<BaseService> _logger;
		private readonly NotificationService _notifService;
		private readonly DialogService _dialogService;

		public BaseService(IHttpClientFactory httpClientFactory, ILogger<BaseService> logger, ITokenService tokenService, NotificationService notifService, DialogService dialogService)
		{
			_tokenService = tokenService;
			_httpClientFactory = httpClientFactory;
			_logger = logger;
			_notifService = notifService;
			_dialogService = dialogService;
		}

		/// <summary>
		/// Send an Http Request to the specified endpoint
		/// </summary>
		/// <typeparam name="TResult">This is the converted returned object</typeparam>
		/// <param name="requestDto"></param>
		/// <param name="withBearer"></param>
		/// <returns></returns>
		public async Task<ResponseDto<TResult>> SendAsync<TResult>(
			RequestDto requestDto, 
			bool withBearer = true,
			Func<RequestDto, Task>? onBeforeSendCallBack = null,
			Func<TResult, Task>? onSuccessSendCallBack = null,
			Func<RequestDto, Task>? onErrorSendCallBack = null)
		{
			try
			{
				HttpClient client = _httpClientFactory.CreateClient("vstecs-aras-client");
				HttpRequestMessage message = new();
				ResponseDto<TResult> responseDto = new();

				if (withBearer)
				{
					var token = _tokenService.GetToken();
					message.Headers.Add("Authorization", $"Bearer {token}");
				}

				message.Headers.Add("Accept", "application/json");

				message.RequestUri = new Uri(requestDto.URL);

				if (requestDto.Data != null)
					message.Content = new StringContent(JsonConvert.SerializeObject(requestDto.Data), encoding: Encoding.UTF8, "application/json");

				HttpResponseMessage? apiResponse = null;

				message.Method = requestDto.ApiType switch
				{
					ApiType.POST => HttpMethod.Post,
					ApiType.DELETE => HttpMethod.Delete,
					ApiType.PUT => HttpMethod.Put,
					_ => HttpMethod.Get,
				};

				if (onBeforeSendCallBack != null)
					await onBeforeSendCallBack(requestDto);

				apiResponse = await client.SendAsync(message);

				switch (apiResponse.StatusCode)
				{
					case HttpStatusCode.NotFound:
						await Notify(requestDto, apiResponse, "Not Found");
						return new() { IsSuccess = false, Message = "Not Found" };

					case HttpStatusCode.Forbidden:
						await Notify(requestDto, apiResponse, "Access Denied", false);
						return new() { IsSuccess = false, Message = "Access Denied" };

					case HttpStatusCode.Unauthorized:
						await Notify(requestDto, apiResponse, "Unauthorized", false);
						return new() { IsSuccess = false, Message = "Unauthorized" };

					case HttpStatusCode.InternalServerError:
						await Notify(requestDto, apiResponse, "Internal Server Error", false);
						return new() { IsSuccess = false, Message = "Internal Server Error! Please check logs for more info." };

					case HttpStatusCode.BadRequest:
						await Notify(requestDto, apiResponse, "Bad Request", false);
						return new() { IsSuccess = false, Message = "Bad Request" };

					default:
						var apiContent = await apiResponse.Content.ReadAsStringAsync();
						var contentResponse = JsonConvert.DeserializeObject<ResponseDto<TResult>>(apiContent) ?? throw new Exception("Error while connecting to the server please check the internet connection");

						if (contentResponse.Result != null && onSuccessSendCallBack != null)
							await onSuccessSendCallBack(contentResponse.Result);

						return contentResponse;
				}
			}
			catch (Exception ex)
			{
				Notify(requestDto, ex, "System Error");

				if (onErrorSendCallBack != null)
					await onErrorSendCallBack(requestDto);

				return new()
				{
					Message = Exceptions.GetMessage(ex),
					IsSuccess = false,
				};
			}
		}

		private async Task Notify(RequestDto request, HttpResponseMessage response, string responseTitle, bool isDebug = true)
		{
			string content = await response.Content.ReadAsStringAsync();
			_logger.Log(isDebug ? LogLevel.Debug : LogLevel.Error, $"{request.URL} {responseTitle} {response.StatusCode} {content}");

			_notifService.Notify(new NotificationMessage
			{
				Summary = "Bad Request",
				Detail = response.RequestMessage.ToString(),
				Duration = 6000,
				Severity = NotificationSeverity.Error,
				Click = async (msg) => await Alert(responseTitle, content)
			});
		}

		private void Notify(RequestDto request, Exception ex, string responseTitle, bool isDebug = true)
		{
			var message = Exceptions.GetMessage(ex);
			_logger.Log(isDebug ? LogLevel.Debug : LogLevel.Error, $"{responseTitle} {message}");

			_notifService.Notify(new NotificationMessage
			{
				Summary = responseTitle,
				Detail = message,
				Duration = 6000,
				Severity = NotificationSeverity.Error,
				Click = async (msg) => await Alert(responseTitle, message)
			});
		}

		private async Task Alert(string title, string message)
		{
			await _dialogService.Alert(message, title);
		}
	}
}
