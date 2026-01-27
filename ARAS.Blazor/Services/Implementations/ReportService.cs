using ARAS.Blazor.Models.DTOs;
using ARAS.Blazor.Services.Interfaces;

namespace ARAS.Blazor.Services.Implementations
{
	public class ReportService : IReportService
	{
		private readonly IBaseService _baseService;

		public ReportService(IBaseService baseService)
		{
			_baseService = baseService;
		}

		public async Task<IEnumerable<ReportsDto>> GetLatestRequestsAsync()
		{
			var resp = await _baseService.SendAsync<IEnumerable<ReportsDto>>(new RequestDto()
			{
				URL = "/api/reports/latest-requests"
            });

			return resp.Result ?? Enumerable.Empty<ReportsDto>();
		}
	}
}