using ARAS.Main.SSMS.Api.Models.Dtos;

namespace ARAS.Main.SSMS.Api.Services.Interfaces
{
	public interface IEmailService
	{
		Task<TaskResultDto> SendRequestPending(ProceedEmailDto emailModel);
		Task<TaskResultDto> SendRequestApproved(ProceedEmailDto emailModel);
		Task<TaskResultDto> SendRequestValidated(ProceedEmailDto emailModel);
		Task<TaskResultDto> SendRequestDeclined(NegateEmailDto emailModel);
		Task<TaskResultDto> SendRequestRejected(NegateEmailDto emailModel);
		Task<TaskResultDto> SendRequestUpdated(UpdateEmailDto emailModel);
		Task<TaskResultDto> Test();
	}
}
