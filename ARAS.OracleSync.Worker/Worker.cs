using ARAS.OracleSync.Worker.App_Code;
using ARAS.OracleSync.Worker.App_Code.Enums;
using ARAS.OracleSync.Worker.Implementations;
using ARAS.OracleSync.Worker.Interfaces;
using ARAS.OracleSync.Worker.Models.DTOs;
using Newtonsoft.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

				var approvedAdjustmentIds = (await _baseService.SendAsync<IEnumerable<long>>(new()
				{
					URL = _config.GetApprovalsUrl()
				})).Result;

				_logger.LogInformation(JsonConvert.SerializeObject(approvedAdjustmentIds));

				var postedAdjustmentIds = (await _baseService.SendAsync<IEnumerable<PostedResponseDto>>(new()
				{
					URL = _config.GetOracleAdjustmentsApiUrl("stage/posted"),
					Data = approvedAdjustmentIds,
					ApiType = App_Code.Enums.ApiType.POST
				})).Result;

				var postedIds = postedAdjustmentIds.Select(x => x.HeaderId);
				var unPostedIds = approvedAdjustmentIds.Except(postedIds).ToList();

				_logger.LogInformation(JsonConvert.SerializeObject(postedIds));

				if(unPostedIds.Any())
				{
					var fetchUnpostedDetailsResponse = await _baseService.SendAsync<IEnumerable<AdjustmentPostingDto>>(new RequestDto<IEnumerable<long>>()
					{
						URL = _config.GetSSMSAdjustmentsApiUrl($"stage/receipt/adjustments"),
						Data = unPostedIds,
						ApiType = ApiType.POST
					});

					if(!fetchUnpostedDetailsResponse.IsSuccess) continue;

					var newlyPostedAdjustmentIds = await _baseService.SendAsync<string>(new RequestDto<IEnumerable<AdjustmentPostingDto>>()
					{
						URL = _config.GetOracleAdjustmentsApiUrl("stage"),
						Data = fetchUnpostedDetailsResponse.Result,
						ApiType = ApiType.POST
					});
				}

				if (postedAdjustmentIds != null && postedAdjustmentIds.Any())
				{
					var statusPosted = await _baseService.SendAsync<string>(new RequestDto<IEnumerable<long>>()
					{
						URL = _config.GetSSMSAdjustmentsApiUrl("stage"),
						Data = approvedAdjustmentIds,
						ApiType = ApiType.POST
					});
					_logger.LogInformation(JsonConvert.SerializeObject(statusPosted.Result));
				}

				await Task.Delay(_config.GetRefreshTime(), stoppingToken);
			}
		}
	}
}
