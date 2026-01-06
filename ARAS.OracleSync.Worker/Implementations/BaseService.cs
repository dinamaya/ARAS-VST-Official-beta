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
				ResponseDto<TResult> responseDto = new();

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
							await Notify(requestDto, apiResponse, "Bad Request", false);
							return new() { IsSuccess = false, Message = "Bad Request" };
						}

					default:
						var contentType = apiResponse.Content.Headers.ContentType?.MediaType;

						var apiContent = await apiResponse.Content.ReadAsStringAsync();
						var contentResponse = JsonConvert.DeserializeObject<ResponseDto<TResult>>(apiContent) ?? throw new Exception("Error while connecting to the server please check the internet connection");

						return contentResponse;
				}
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
