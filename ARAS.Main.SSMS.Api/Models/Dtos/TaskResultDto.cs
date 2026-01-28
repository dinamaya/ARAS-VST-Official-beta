namespace ARAS.Main.SSMS.Api.Models.Dtos
{
	public class TaskResultDto
	{
		public string Message { get; set; }
		public bool IsSuccess { get; set; } = false;
		public bool IsCompleted { get; set; } = false;

		public static TaskResultDto Success(string message) => new TaskResultDto { Message = message, IsSuccess = true, IsCompleted = true };
		public static TaskResultDto Fail(string message) => new TaskResultDto { Message = message, IsSuccess = false, IsCompleted = true };
		public bool IsOk() => IsSuccess && IsCompleted;
	}
}
