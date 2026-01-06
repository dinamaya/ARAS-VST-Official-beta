using ARAS.Blazor.App_Code.Globals;
using ARAS.Blazor.Models.DTOs;
using ARAS.Blazor.Services.Interfaces;
using ARAS.Main.Oracle.Api.Models.Dtos;

namespace ARAS.Blazor.Services.Implementations
{
	public class OracleStagingService(IBaseService baseService, IConfigService configService) : IOracleStagingService
	{
		public async Task Create(AdjustmentPostingDto data)
		{
			var response = await baseService.SendAsync<IEnumerable<InvoiceDetailsDto>>(new RequestDto()
			{
				URL = configService.GetOracleAdjustmentApiUrl(),
				Data = data
			});

			Guards.ThrowNullReferenceIf(response?.Result, response.Message);
		}
	}
}
