using ARAS.Main.SSMS.Api.Models.Dtos;

namespace ARAS.Main.SSMS.Api.Services.Interfaces
{
	/// <summary>
	/// Prefix the Methods with "Run" to indicate they initiate background jobs.
	/// </summary>
	public interface IBackgroundJobService
	{
		public Task<string> RunTest();
		public Task RunSendRequestPending(ProceedEmailDto emailModel);
		public Task RunSendRequestApproved(ProceedEmailDto emailModel);
		public Task RunSendRequestValidated(ProceedEmailDto emailModel);
		public Task RunSendRequestUpdated(UpdateEmailDto emailModel);
		public Task RunSendRequestDeclined(NegateEmailDto emailModel);
		public Task RunSendRequestRejected(NegateEmailDto emailModel);
	}
}
