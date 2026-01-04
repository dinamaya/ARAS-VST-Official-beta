using ARAS.OracleSync.Worker.App_Code.Enums;

namespace ARAS.OracleSync.Worker.Models.DTOs
{
	public class RequestDto
	{
		public ApiType ApiType { get; set; } = ApiType.GET;
		public string URL { get; set; }
		public object Data { get; set; }
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
