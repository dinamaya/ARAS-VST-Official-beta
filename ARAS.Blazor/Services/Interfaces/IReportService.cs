using ARAS.Blazor.Models.DTOs;

namespace ARAS.Blazor.Services.Interfaces
{
	public interface IReportService
	{
		Task<IEnumerable<ReportsDto>> GetLatestRequestsAsync();
	}
}
