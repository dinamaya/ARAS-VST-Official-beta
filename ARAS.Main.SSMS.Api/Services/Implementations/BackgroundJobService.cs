using Hangfire;
using ARAS.Main.SSMS.Api.Services.Interfaces;
using Microsoft.AspNetCore.Http;

namespace ARAS.Main.SSMS.Api.Services.Implementations
{
	public class BackgroundJobService : IBackgroundJobService
	{
		private readonly ILogger<BackgroundJobService> _logger;

		public BackgroundJobService(ILogger<BackgroundJobService> logger)
		{
			_logger = logger;
		}

		public async Task<string> RunTestJob()
		{
			BackgroundJob.Enqueue<IEmailService>((service) => service.Test());
			return "Done";
		}
	}
}
