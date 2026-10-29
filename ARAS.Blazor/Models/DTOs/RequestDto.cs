using ARAS.Blazor.App_Code.Globals.Enums;
using System.Security.AccessControl;

namespace ARAS.Blazor.Models.DTOs
{
	public class RequestDto
	{
		public ApiType ApiType { get; set; } = ApiType.GET;
		public string URL { get; set; }
		public object Data { get; set; }
		public string AccessToken { get; set; }
		public ContentType ContentType { get; set; } = ContentType.Json;
		/// <summary>
		/// Use this property when ContentType is MultipartFormData and Data is a collection
		/// </summary>
		public string? FormCollectionName { get; set; }
		/// <summary>
		/// Named HttpClient to send this request with. Leave null for the default
		/// client; use <see cref="PostingHttpClient"/> for slow Oracle Fusion postings.
		/// </summary>
		public string? HttpClientName { get; set; }

		public const string DefaultHttpClient = "vstecs-aras-client";
		public const string PostingHttpClient = "vstecs-aras-posting";
	}


	public class RequestDto<T> : RequestDto
	{
		public new T? Data
		{
			get => (T?)base.Data;
			set => base.Data = value;
		}
	}

}
