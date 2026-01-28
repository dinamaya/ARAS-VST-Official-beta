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
			return "Done";
		}
	}
}
