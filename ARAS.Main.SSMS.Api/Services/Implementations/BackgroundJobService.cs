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

		public async Task RunSendRequestPending(ProceedEmailDto emailModel)
		{
			await Task.Run(() =>
			{
				BackgroundJob.Enqueue<IEmailService>((service) => service.SendRequestPending(emailModel));
			});
		}

		public async Task RunSendRequestApproved(ProceedEmailDto emailModel)
		{
			await Task.Run(() =>
			{
				BackgroundJob.Enqueue<IEmailService>((service) => service.SendRequestApproved(emailModel));
			});
		}

		public async Task RunSendRequestValidated(ProceedEmailDto emailModel)
		{
			await Task.Run(() => 
			{
				BackgroundJob.Enqueue<IEmailService>((service) => service.SendRequestValidated(emailModel));
			});
		}

		public async Task RunSendRequestDeclined(NegateEmailDto emailModel)
		{
			await Task.Run(() => 
			{ 
				BackgroundJob.Enqueue<IEmailService>((service) => service.SendRequestDeclined(emailModel)); 
			});
		}

		public async Task RunSendRequestRejected(NegateEmailDto emailModel)
		{
			await Task.Run(() => 
			{ 
				BackgroundJob.Enqueue<IEmailService>((service) => service.SendRequestRejected(emailModel)); 
			});
		}

		public async Task RunSendRequestUpdated(UpdateEmailDto emailModel)
		{
			await Task.Run(() =>
			{
				BackgroundJob.Enqueue<IEmailService>((service) => service.SendRequestUpdated(emailModel)); 
			});
		}
	}
}
