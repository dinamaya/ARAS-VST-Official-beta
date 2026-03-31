using ARAS.Blazor.Models.DTOs;

namespace ARAS.Blazor.Services.Interfaces
{
    public interface IReportService
    {
        Task<IEnumerable<ReportsDto>> GetReportsAsync(ReportFiltersDto filters);
        Task ExportExcelAsync(IEnumerable<ReportsDto> rows, ReportFiltersDto filters);
    }
}
