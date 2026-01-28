using ARAS.Blazor.App_Code.Globals;
using ARAS.Blazor.App_Code.Globals.Constants;
using ARAS.Blazor.App_Code.Globals.Enums;
using ARAS.Blazor.Models.DTOs;
using ARAS.Blazor.Services.Interfaces;
using Azure;
using System.Collections;
using System.Net.Http.Headers;

using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
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
		private readonly IValidationService _validationService;
		private readonly IAuthService _authService;

		private readonly ILogger<BaseService> _logger;
		private readonly NotificationService _notifService;
		private readonly DialogService _dialogService;

		public BaseService(IHttpClientFactory httpClientFactory, ILogger<BaseService> logger, ITokenService tokenService, NotificationService notifService, DialogService dialogService, IValidationService validationService, IAuthService authService)
		{
			_tokenService = tokenService;
			_httpClientFactory = httpClientFactory;
			_logger = logger;
			_notifService = notifService;
			_dialogService = dialogService;
			_validationService = validationService;
			_authService = authService;
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
			Func<ResponseDto<TResult>, Task>? onSuccessSendCallBack = null,
			Func<RequestDto, Task>? onErrorSendCallBack = null)
		{
			try
			{
				//bool isSecurityHashValid = await _authService.IsAccountSecurityHashValid();
				//Guards.ThrowInvalidOperationIf(!isSecurityHashValid, "Session expired. Please log in again.");

				HttpClient client = _httpClientFactory.CreateClient("vstecs-aras-client");
				HttpRequestMessage message = new();
				ResponseDto<TResult> responseDto = new();

				_validationService.ClearErrors();

				if (requestDto.ContentType == ContentType.MultipartFormData)
					message.Headers.Add("Accept", "*/*");
				else
					message.Headers.Add("Accept", "application/json");

				if (withBearer)
				{
					var token = _tokenService.GetToken();
					message.Headers.Add("Authorization", $"Bearer {token}");
				}

				message.RequestUri = new Uri(requestDto.URL);

				if (requestDto.ContentType == ContentType.MultipartFormData)
				{
					var content = new MultipartFormDataContent();

					if (requestDto.Data is IEnumerable enumerable && requestDto.Data is not string)
					{
						int index = 0;
						foreach (var item in enumerable)
						{
							if (item == null) continue;
							var props = item.GetType().GetProperties().Where(p => p.GetIndexParameters().Length == 0);
							foreach (var prop in props) 
							{
								var value = prop.GetValue(item);
								AddFormDataValue(content, $"{requestDto.FormCollectionName}[{index}].{prop.Name}", value);
							}

							index++;
						}
					}
					else
					{
						// Handle a single object
						var props = requestDto.Data.GetType().GetProperties().Where(p => p.GetIndexParameters().Length == 0);
						foreach (var prop in props)
						{
							var value = prop.GetValue(requestDto.Data);
							AddFormDataValue(content, prop.Name, value);
						}
					}

					message.Content = content;
				}
				else
				{
					if (requestDto.Data != null)
						message.Content = new StringContent(JsonConvert.SerializeObject(requestDto.Data), Encoding.UTF8, "application/json");
				}


				HttpResponseMessage? apiResponse = null;

				message.Method = requestDto.ApiType switch
				{
					ApiType.POST => HttpMethod.Post,
					ApiType.DELETE => HttpMethod.Delete,
					ApiType.PUT => HttpMethod.Put,
					_ => HttpMethod.Get,
				};

				if (onBeforeSendCallBack != null)
					await onBeforeSendCallBack.Invoke(requestDto);

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
						{
							await NotifyBadRequest(requestDto, apiResponse);
							return new() { IsSuccess = false, Message = "Bad Request" };
						}

					default:
						var contentType = apiResponse.Content.Headers.ContentType?.MediaType;

						if (contentType != null && !contentType.Contains("application/json", StringComparison.OrdinalIgnoreCase))
							return await DownloadMedia<TResult>(apiResponse);

						var apiContent = await apiResponse.Content.ReadAsStringAsync();
						var contentResponse = JsonConvert.DeserializeObject<ResponseDto<TResult>>(apiContent) ?? throw new Exception("Error while connecting to the server please check the internet connection");

						if (contentResponse.Result != null && onSuccessSendCallBack != null)
							await onSuccessSendCallBack.Invoke(contentResponse);

						return contentResponse;
				}
			}
			catch (Exception ex)
			{
				Notify(requestDto, ex, "System Error");

				if (onErrorSendCallBack != null)
					await onErrorSendCallBack.Invoke(requestDto);

				return new()
				{
					Message = Exceptions.GetMessage(ex),
					IsSuccess = false,
				};
			}
		}

		private async Task NotifyBadRequest(RequestDto requestDto, HttpResponseMessage apiResponse)
		{
			try
			{
				var content = await apiResponse.Content.ReadAsStringAsync();
				var validationProblem = JsonConvert.DeserializeObject<ValidationProblemDetails>(content);

				if (!validationProblem.Title.Equals("One or more validation errors occurred.", StringComparison.InvariantCultureIgnoreCase))
					throw new Exception();

				if (validationProblem?.Errors != null)
				{
					var validationResult = validationProblem.Errors
						.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToList());

					_validationService.DisplayErrors(validationResult);
					await Notify(requestDto, apiResponse, validationResult, "Bad Request", false);
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error parsing validation response.");

				await Notify(requestDto, apiResponse, "Bad Request", false);
			}
		}

		private async Task Notify(RequestDto request, HttpResponseMessage response, Dictionary<string, List<string>> errors, string responseTitle, bool isDebug = true)
		{
			string content = await response.Content.ReadAsStringAsync();
			foreach (var (k, v) in errors)
			{
				string message = v.FirstOrDefault();
				string notifMessage = $"{request.URL} {responseTitle} {response.StatusCode} {content}";

				_logger.Log(LogLevel.Error, $"{request.URL} {responseTitle} {k} {message}");
				_notifService.Notify(NotifTemplates.Error(responseTitle, message, async (msg) => await Alert(responseTitle, notifMessage)));
			}
		}

		private async Task Notify(RequestDto request, HttpResponseMessage response, string responseTitle, bool isDebug = true)
		{
			string content = await response.Content.ReadAsStringAsync();
			_logger.Log(isDebug ? LogLevel.Debug : LogLevel.Error, $"{request.URL} {responseTitle} {response.StatusCode} {content}");
			_notifService.Notify(NotifTemplates.Error(responseTitle, content, async (msg) => await Alert(responseTitle, content)));
		}

		private void Notify(RequestDto request, Exception ex, string responseTitle, bool isDebug = true)
		{
			var message = Exceptions.GetMessage(ex);
			_logger.Log(isDebug ? LogLevel.Debug : LogLevel.Error, $"{responseTitle} {message}");
			_notifService.Notify(NotifTemplates.Error(responseTitle, message, async (msg) => await Alert(responseTitle, message)));
		}

		private async Task Alert(string title, string message)
		{
			await _dialogService.Alert(message, title);
		}

		private static void AddFormDataValue(MultipartFormDataContent content, string name, object? value)
		{
			switch (value)
			{
				case null:
					content.Add(new StringContent(string.Empty), name);
					break;

				case byte[] bytes:
					var byteContent = new ByteArrayContent(bytes);
					byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
					content.Add(byteContent, name, name);
					break;

				default:
					content.Add(new StringContent(value.ToString() ?? string.Empty), name);
					break;
			}
		}

		private async Task<ResponseDto<TResult>> DownloadMedia<TResult>(HttpResponseMessage apiResponse)
		{
			var fileBytes = await apiResponse.Content.ReadAsByteArrayAsync();

			var fileName = apiResponse.Content.Headers.ContentDisposition?.FileName?.Trim('"') ?? "downloaded_file";

			object? result = default(TResult);
			if (typeof(TResult) == typeof(byte[]))
				result = (object)fileBytes;

			var responseDto = new ResponseDto<TResult>
			{
				Result = (TResult)result!,
				IsSuccess = true,
				Message = fileName
			};

			return responseDto;
		}
	}
}
