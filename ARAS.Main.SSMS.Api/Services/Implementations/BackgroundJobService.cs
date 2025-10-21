using ARAS.Main.SSMS.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.Services.Interfaces;
using Hangfire;
using Microsoft.AspNetCore.Http;

namespace ARAS.Main.SSMS.Api.Services.Implementations
{
	/// <summary>
	/// Prefix the Methods with "Run" to indicate they initiate background jobs.
	/// </summary>
	public class BackgroundJobService : IBackgroundJobService
	{
		private readonly ILogger<BackgroundJobService> _logger;

		public BackgroundJobService(ILogger<BackgroundJobService> logger)
		{
			_logger = logger;
		}

		public async Task<string> RunTest()
		{
			BackgroundJob.Enqueue<IEmailService>((service) => service.Test());
			return "Done";
		}

		public async Task RunSendRequestPending(RequestPendingDto emailModel)
		{
			BackgroundJob.Enqueue<IEmailService>((service) => service.SendRequestPending(emailModel));
		}

		public async Task RunSendRequestApproved(RequestPendingDto emailModel)
		{
			BackgroundJob.Enqueue<IEmailService>((service) => service.SendRequestApproved(emailModel));
		}
	}
}
