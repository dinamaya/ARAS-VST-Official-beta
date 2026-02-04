using ARAS.OracleSync.Worker.Interfaces;
using ARAS.OracleSync.Worker.Models.DTOs;
using Newtonsoft.Json;

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

				var approvedAdjustmentIds = await _baseService.SendAsync<IEnumerable<long>>(new()
				{
					URL = _config.GetApprovalsUrl()
				});

				_logger.LogInformation(JsonConvert.SerializeObject(approvedAdjustmentIds.Result));

				var postedAdjustmentIds = await _baseService.SendAsync<IEnumerable<PostedResponseDto>>(new()
				{
					URL = _config.GetOracleAdjustmentsApiUrl("stage/posted"),
					Data = approvedAdjustmentIds.Result,
					ApiType = App_Code.Enums.ApiType.POST
				});

				_logger.LogInformation(JsonConvert.SerializeObject(postedAdjustmentIds.Result));

				if (postedAdjustmentIds.Result != null && postedAdjustmentIds.Result.Any())
				{
					var statusPosted = await _baseService.SendAsync<string>(new RequestDto<IEnumerable<long>>()
					{
						URL = _config.GetSSMSAdjustmentsApiUrl("stage"),
						Data = approvedAdjustmentIds.Result,
						ApiType = App_Code.Enums.ApiType.POST
					});
					_logger.LogInformation(JsonConvert.SerializeObject(statusPosted.Result));
				}

				await Task.Delay(_config.GetRefreshTime(), stoppingToken);
			}
		}
	}
}
