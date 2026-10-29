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

        public async Task<IEnumerable<string>> GetInvoiceTypes()
        {
			var response = await _baseService.SendAsync<IEnumerable<string>>(new RequestDto()
			{
				URL = _configService.GetAdjustmentsUrl("types/invoice"),
			});

			Guards.ThrowInvalidOperationIf(!response.IsSuccess, "Failed to connect. Please Check internet connection or contact the administrator");

			return (response.Result ?? [])
				.Where(type => !HiddenInvoiceTypes.Contains(type))
				.ToList();
		}

		/// <summary>
		/// Invoice adjustment types that are not available yet (planned for a later version).
		/// Hidden from every invoice type list: file request, approvals and request history.
		/// Remove a name from this list to make the type available again.
		/// </summary>
		private static readonly HashSet<string> HiddenInvoiceTypes = new(StringComparer.OrdinalIgnoreCase)
		{
			"Write-Off: Sales Return Auto Net",
			"AR Invoice Offsetting"
		};
    }
}
