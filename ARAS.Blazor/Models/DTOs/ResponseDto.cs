namespace ARAS.Blazor.Models.DTOs
{
	public class ResponseDto
	{
		public object? Result { get; set; } = null;
		public bool IsSuccess { get; set; } = true;
		public string Message { get; set; } = string.Empty;
	}

	public class ResponseDto<T> : ResponseDto
	{
		public new T? Result
		{
			get => (T?)base.Result;
			set => base.Result = value;
		}

		public ResponseDto<T> Failed(string message)
		{
			IsSuccess = false;
			Message = message;
			Result = default(T);
			return this;
		}
	}
}
