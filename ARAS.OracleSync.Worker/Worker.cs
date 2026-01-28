using ARAS.OracleSync.Worker.Interfaces;
using ARAS.OracleSync.Worker.Models.DTOs;

namespace ARAS.OracleSync.Worker
{
	public class Worker : BackgroundService
	{
		private readonly IBaseService _baseService;
		private readonly ILogger<Worker> _logger;
		private readonly IConfigService _config;

		public Worker(ILogger<Worker> logger, IBaseService baseService, IConfigService config)
		{
			_logger = logger;
			_baseService = baseService;
			_config = config;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				// TODO: Implement Fetching and then updating of the Main SSMS Request Rows' Status to Posted here
				// Fetch all adjustments in staging table that already has 3 True Flags
				// Fetch all adjustments in Validated adjustments in Main table
				// Update the Status of those adjustments to Posted
				var response = await _baseService.SendAsync<IEnumerable<string>>(new RequestDto()
				{
					ApiType = App_Code.Enums.ApiType.GET,
					URL = _config.GetOracleAdjustmentsApiUrl("reason-codes")
				});

				_logger.LogInformation($"Success Result Oracle Connection");

				foreach(string code in response.Result)
				{
					_logger.LogInformation(code);
				}

				await Task.Delay(10000, stoppingToken);
			}
		}
	}
}
