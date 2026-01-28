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
