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
				try
				{
	                _logger.LogInformation("Worker polling cycle started.");

	                // Fetch all adjustments in staging table that already has 3 True Flags
	                // Fetch all adjustments in Validated adjustments in Main table
	                // Update the Status of those adjustments to Posted

	                var approvedResponse = await _baseService.SendAsync<IEnumerable<ApprovedAdjustmentSyncDto>>(new()
					{
						URL = _config.GetApprovalsUrl("sync")
					});

	                var approvedAdjustments = approvedResponse.Result?.ToList() ?? [];
	                _logger.LogInformation("Approved adjustments for sync: {Count}", approvedAdjustments.Count);

	                if (!approvedAdjustments.Any())
					{
	                    await Task.Delay(_config.GetRefreshTime(), stoppingToken);
	                    continue;
	                }

	                var approvedHeaderIds = approvedAdjustments.Select(x => x.HeaderId).Distinct().ToList();
	                _logger.LogInformation("Approved header IDs: {HeaderIds}", JsonConvert.SerializeObject(approvedHeaderIds));

	                var postedResponse = await _baseService.SendAsync<IEnumerable<PostedResponseDto>>(new()
					{
						URL = _config.GetOracleAdjustmentsApiUrl("stage/posted"),
	                    Data = approvedHeaderIds,
	                    ApiType = App_Code.Enums.ApiType.POST
					});

					if (!postedResponse.IsSuccess)
					{
						_logger.LogWarning("Failed to fetch posted header IDs from Oracle. Response: {Message}", postedResponse.Message);
						await Task.Delay(_config.GetRefreshTime(), stoppingToken);
						continue;
					}

					var postedAdjustmentIds = postedResponse.Result?.ToList() ?? [];
					var postedIds = postedAdjustmentIds.Select(x => x.HeaderId).ToList();
	                var unPostedIds = approvedHeaderIds.Except(postedIds).ToList();

	                _logger.LogInformation("Posted header IDs returned by Oracle: {HeaderIds}", JsonConvert.SerializeObject(postedIds));
	                _logger.LogInformation("Unposted header IDs to restage: {HeaderIds}", JsonConvert.SerializeObject(unPostedIds));

					if(unPostedIds.Any())
					{
						var fetchUnpostedDetailsResponse = await _baseService.SendAsync<IEnumerable<AdjustmentPostingDto>>(new RequestDto<IEnumerable<long>>()
						{
							URL = _config.GetSSMSAdjustmentsApiUrl($"stage/receipt/adjustments"),
							Data = unPostedIds,
							ApiType = ApiType.POST
						});

						if(!fetchUnpostedDetailsResponse.IsSuccess)
						{
							_logger.LogWarning("Failed to fetch staging details for unposted header IDs.");
							await Task.Delay(_config.GetRefreshTime(), stoppingToken);
							continue;
						}

						var newlyPostedAdjustmentIds = await _baseService.SendAsync<string>(new RequestDto<IEnumerable<AdjustmentPostingDto>>()
						{
							URL = _config.GetOracleAdjustmentsApiUrl("stage"),
							Data = fetchUnpostedDetailsResponse.Result,
							ApiType = ApiType.POST
						});

						if (!newlyPostedAdjustmentIds.IsSuccess)
						{
							_logger.LogWarning("Failed to restage unposted header IDs. Response: {Message}", newlyPostedAdjustmentIds.Message);
							await Task.Delay(_config.GetRefreshTime(), stoppingToken);
							continue;
						}

						_logger.LogInformation("Restage response: {Result}", JsonConvert.SerializeObject(newlyPostedAdjustmentIds.Result));
					}

					if (postedAdjustmentIds.Any())
					{
	                    var postedHeaderSet = postedIds.ToHashSet();
	                    var postedRequestIds = approvedAdjustments
	                        .Where(x => postedHeaderSet.Contains(x.HeaderId))
	                        .Select(x => x.RequestId)
	                        .Distinct()
	                        .ToList();

	                    _logger.LogInformation("Request IDs to mark as Posted: {RequestIds}", JsonConvert.SerializeObject(postedRequestIds));

	                    var statusPosted = await _baseService.SendAsync<string>(new RequestDto<IEnumerable<long>>()
						{
	                        URL = _config.GetSSMSAdjustmentsApiUrl("stage/requests"),
	                        Data = postedRequestIds,
	                        ApiType = ApiType.POST
						});

						if (!statusPosted.IsSuccess)
						{
							_logger.LogWarning("Failed to update posted request statuses. Response: {Message}", statusPosted.Message);
							await Task.Delay(_config.GetRefreshTime(), stoppingToken);
							continue;
						}

						_logger.LogInformation("Posted status update response: {Result}", JsonConvert.SerializeObject(statusPosted.Result));
					}
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Worker polling cycle failed.");
				}
				await Task.Delay(_config.GetRefreshTime(), stoppingToken);
			}
		}
	}
}
