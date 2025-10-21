using ARAS.Main.SSMS.Api.Models.Dtos;

namespace ARAS.Main.SSMS.Api.Services.Interfaces
{
	/// <summary>
	/// Prefix the Methods with "Run" to indicate they initiate background jobs.
	/// </summary>
	public interface IBackgroundJobService
	{
		public Task<string> RunTest();
		public Task RunSendRequestPending(RequestPendingDto emailModel);
		public Task RunSendRequestApproved(RequestPendingDto emailModel);
	}
}
