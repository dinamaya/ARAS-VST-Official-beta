using ARAS.Main.SSMS.Api.Models.Dtos;

namespace ARAS.Main.SSMS.Api.Services.Interfaces
{
	public interface IEmailService
	{
		Task<TaskResultDto> SendRequestPending(RequestPendingDto emailModel);
		Task<TaskResultDto> SendRequestApproved(RequestPendingDto emailModel);
		Task<TaskResultDto> Test();
	}
}
