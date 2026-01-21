using ARAS.Blazor.App_Code.Globals;
using ARAS.Blazor.App_Code.Globals.Enums;
using ARAS.Blazor.Models.DTOs;
using ARAS.Blazor.Services.Interfaces;

namespace ARAS.Blazor.Services.Implementations
{
    public class AdjustmentService : IAdjustmentService
    {
		private readonly IBaseService _baseService;
		private readonly IConfigService _configService;

        public AdjustmentService(IBaseService baseService, IConfigService configService)
        {
            _baseService = baseService;
            _configService = configService;
        }

        public async Task<IEnumerable<string>> GetAdjustmentTypes()
        {
			var response = await _baseService.SendAsync<IEnumerable<string>>(new RequestDto()
			{
				URL = _configService.GetAdjustmentsUrl("types"),
			});

			Guards.ThrowInvalidOperationIf(!response.IsSuccess, "Failed to connect. Please Check internet connection or contact the administrator");

			return response.Result;
		}

        public async Task<IEnumerable<string>> GetReceiptTypes()
        {
			var response = await _baseService.SendAsync<IEnumerable<string>>(new RequestDto()
			{
				URL = _configService.GetAdjustmentsUrl("types/receipt"),
			});

			Guards.ThrowInvalidOperationIf(!response.IsSuccess, "Failed to connect. Please Check internet connection or contact the administrator");

			return response.Result;
		}
    }
}
