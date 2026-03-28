using ARAS.OracleSync.Worker.App_Code.Constants;
using ARAS.OracleSync.Worker.App_Code.Enums;
using ARAS.OracleSync.Worker.Interfaces;
using ARAS.OracleSync.Worker.Models.DTOs;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net;
using System.Text;

namespace ARAS.OracleSync.Worker.Implementations
{
	public class BaseService : IBaseService
	{
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly ILogger<BaseService> _logger;

		public BaseService(IHttpClientFactory httpClientFactory, ILogger<BaseService> logger)
		{
			_httpClientFactory = httpClientFactory;
			_logger = logger;
		}

		/// <summary>
		/// Send an Http Request to the specified endpoint
		/// </summary>
		/// <typeparam name="TResult">This is the converted returned object</typeparam>
		/// <param name="requestDto"></param>
		/// <param name="withBearer"></param>
		/// <returns></returns>
		public async Task<ResponseDto<TResult>> SendAsync<TResult>(RequestDto requestDto, bool withBearer = true)
		{
			try
			{
				HttpClient client = _httpClientFactory.CreateClient("vstecs-aras-client");
				HttpRequestMessage message = new();

				message.Headers.Add("Accept", "application/json");

				//if (withBearer)
				//{
					//var token = _tokenService.GetToken();
					//message.Headers.Add("Authorization", $"Bearer {token}");
				//}

				message.RequestUri = new Uri(requestDto.URL);
				if (requestDto.Data != null)
					message.Content = new StringContent(JsonConvert.SerializeObject(requestDto.Data), Encoding.UTF8, "application/json");

				HttpResponseMessage? apiResponse = null;

				message.Method = requestDto.ApiType switch
				{
					ApiType.POST => HttpMethod.Post,
					ApiType.DELETE => HttpMethod.Delete,
					ApiType.PUT => HttpMethod.Put,
					_ => HttpMethod.Get,
				};

				_logger.Log(LogLevel.Debug, $"Request {message.Method.Method} on {requestDto.URL}");
				
				apiResponse = await client.SendAsync(message);

				if (!apiResponse.IsSuccessStatusCode)
				{
					var messageText = apiResponse.StatusCode switch
					{
						HttpStatusCode.NotFound => "Not Found",
						HttpStatusCode.Forbidden => "Access Denied",
						HttpStatusCode.Unauthorized => "Unauthorized",
						HttpStatusCode.InternalServerError => "Internal Server Error! Please check logs for more info.",
						HttpStatusCode.BadRequest => "Bad Request",
						_ => $"HTTP {(int)apiResponse.StatusCode} {apiResponse.ReasonPhrase}"
					};

					await Notify(requestDto, apiResponse, messageText, false);
					return new() { IsSuccess = false, Message = messageText };
				}

				var apiContent = await apiResponse.Content.ReadAsStringAsync();
				var contentResponse = JsonConvert.DeserializeObject<ResponseDto<TResult>>(apiContent) ?? throw new Exception("Error while connecting to the server please check the internet connection");

				return contentResponse;
			}
			catch (Exception ex)
			{
				Notify(requestDto, ex, "System Error");

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
		}

		private void Notify(RequestDto request, Exception ex, string responseTitle, bool isDebug = true)
		{
			var message = Exceptions.GetMessage(ex);
			_logger.Log(isDebug ? LogLevel.Debug : LogLevel.Error, $"{responseTitle} {message}");
		}
	}
}
