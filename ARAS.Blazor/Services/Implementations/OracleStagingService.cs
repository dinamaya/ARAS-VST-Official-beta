using ARAS.Blazor.App_Code.Globals;
using ARAS.Blazor.App_Code.Globals.Enums;
using ARAS.Blazor.Models.DTOs;
using ARAS.Blazor.Services.Interfaces;
using ARAS.Main.Oracle.Api.Models.Dtos;
namespace ARAS.Blazor.Services.Implementations
{
	public class OracleStagingService(IBaseService baseService, IConfigService configService) : IOracleStagingService
	{
		public async Task Create(IEnumerable<AdjustmentPostingDto> data)
		{
			var response = await baseService.SendAsync<string>(new RequestDto<IEnumerable<AdjustmentPostingDto>>()
			{
				URL = configService.GetOracleAdjustmentApiUrl("stage"),
				Data = data,
				ApiType = ApiType.POST
			});

            Guards.ThrowInvalidOperationIf(!(response?.IsSuccess ?? false), response?.Message ?? "Failed to create Oracle staging row.");
            Guards.ThrowNullReferenceIf(response?.Result, response?.Message ?? "Oracle staging returned no result.");
        }
	}
}