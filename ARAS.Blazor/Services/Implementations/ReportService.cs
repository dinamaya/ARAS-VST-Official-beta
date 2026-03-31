using ARAS.Blazor.Models.DTOs;
using ARAS.Blazor.Services.Interfaces;
using Microsoft.JSInterop;
using System.Net;
using System.Text;

namespace ARAS.Blazor.Services.Implementations
{
    public class ReportService : IReportService
    {
        private readonly IBaseService _baseService;
        private readonly IConfigService _configService;
        private readonly IJSRuntime _jsRuntime;

        public ReportService(IBaseService baseService, IConfigService configService, IJSRuntime jsRuntime)
        {
            _baseService = baseService;
            _configService = configService;
            _jsRuntime = jsRuntime;
        }

        public async Task<IEnumerable<ReportsDto>> GetReportsAsync(ReportFiltersDto filters)
        {
            var response = await _baseService.SendAsync<IEnumerable<ReportsDto>>(new RequestDto()
            {
                URL = BuildUrl(filters)
            });

            if (!response.IsSuccess)
                throw new InvalidOperationException(string.IsNullOrWhiteSpace(response.Message) ? "Unable to load reports." : response.Message);

            return response.Result ?? Enumerable.Empty<ReportsDto>();
        }

        public async Task ExportExcelAsync(IEnumerable<ReportsDto> rows, ReportFiltersDto filters)
        {
            var content = BuildExcelCompatibleHtml(rows, filters);
            var bytes = Encoding.UTF8.GetBytes(content);
            var fileName = $"reports-{SanitizeFileName(filters.ReportType)}-{DateTime.Now:yyyyMMddHHmmss}.xls";
            await _jsRuntime.InvokeVoidAsync("downloadFile", fileName, Convert.ToBase64String(bytes));
        }

        private string BuildUrl(ReportFiltersDto filters)
        {
            var query = new Dictionary<string, string?>
            {
                ["reportType"] = filters.ReportType,
                ["startDate"] = filters.StartDate.ToString("yyyy-MM-dd"),
                ["endDate"] = filters.EndDate.ToString("yyyy-MM-dd"),
                ["status"] = filters.Status,
                ["adjustmentType"] = filters.AdjustmentType,
                ["customerName"] = filters.CustomerName,
                ["requestorName"] = filters.RequestorName
            };

            var queryString = string.Join("&",
                query
                    .Where(x => !string.IsNullOrWhiteSpace(x.Value))
                    .Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value!)}"));

            return string.IsNullOrWhiteSpace(queryString)
                ? _configService.GetRequestsUrl("reports")
                : $"{_configService.GetRequestsUrl("reports")}?{queryString}";
        }

        private static string BuildExcelCompatibleHtml(IEnumerable<ReportsDto> rows, ReportFiltersDto filters)
        {
            var builder = new StringBuilder();

            builder.AppendLine("<html><head><meta charset=\"utf-8\" /></head><body>");
            builder.AppendLine("<table border=\"1\">");
            builder.AppendLine($"<tr><td><strong>Report Type</strong></td><td>{Encode(filters.ReportType)}</td></tr>");
            builder.AppendLine($"<tr><td><strong>Date Range</strong></td><td>{filters.StartDate:yyyy-MM-dd} to {filters.EndDate:yyyy-MM-dd}</td></tr>");
            builder.AppendLine($"<tr><td><strong>Status</strong></td><td>{Encode(string.IsNullOrWhiteSpace(filters.Status) ? "All Statuses" : filters.Status)}</td></tr>");
            builder.AppendLine($"<tr><td><strong>Adjustment Type</strong></td><td>{Encode(string.IsNullOrWhiteSpace(filters.AdjustmentType) ? "All Adjustment Types" : filters.AdjustmentType)}</td></tr>");
            builder.AppendLine($"<tr><td><strong>Customer</strong></td><td>{Encode(string.IsNullOrWhiteSpace(filters.CustomerName) ? "All Customers" : filters.CustomerName)}</td></tr>");
            builder.AppendLine($"<tr><td><strong>Requestor</strong></td><td>{Encode(string.IsNullOrWhiteSpace(filters.RequestorName) ? "All Requestors" : filters.RequestorName)}</td></tr>");
            builder.AppendLine("</table><br />");

            builder.AppendLine("<table border=\"1\">");
            builder.AppendLine("<tr>");
            builder.AppendLine("<th>Request No</th>");
            builder.AppendLine("<th>Category</th>");
            builder.AppendLine("<th>Customer</th>");
            builder.AppendLine("<th>Invoice No</th>");
            builder.AppendLine("<th>Adjustment Type</th>");
            builder.AppendLine("<th>Requestor</th>");
            builder.AppendLine("<th>Approver</th>");
            builder.AppendLine("<th>Status</th>");
            builder.AppendLine("<th>Date Requested</th>");
            builder.AppendLine("<th>Date Approved</th>");
            builder.AppendLine("<th>Date Updated</th>");
            builder.AppendLine("<th>Updated By</th>");
            builder.AppendLine("<th>Amount</th>");
            builder.AppendLine("</tr>");

            foreach (var row in rows)
            {
                builder.AppendLine("<tr>");
                builder.AppendLine($"<td>{Encode(row.RequestNumber)}</td>");
                builder.AppendLine($"<td>{Encode(row.Category)}</td>");
                builder.AppendLine($"<td>{Encode(row.CustomerName)}</td>");
                builder.AppendLine($"<td>{Encode(row.InvoiceNumber)}</td>");
                builder.AppendLine($"<td>{Encode(row.AdjustmentType)}</td>");
                builder.AppendLine($"<td>{Encode(row.Requestor)}</td>");
                builder.AppendLine($"<td>{Encode(row.Approver)}</td>");
                builder.AppendLine($"<td>{Encode(row.Status)}</td>");
                builder.AppendLine($"<td>{row.DateRequested:yyyy-MM-dd HH:mm}</td>");
                builder.AppendLine($"<td>{(row.DateApproved.HasValue ? row.DateApproved.Value.ToString("yyyy-MM-dd HH:mm") : string.Empty)}</td>");
                builder.AppendLine($"<td>{row.DateUpdated:yyyy-MM-dd HH:mm}</td>");
                builder.AppendLine($"<td>{Encode(row.UpdatedBy)}</td>");
                builder.AppendLine($"<td>{row.Amount:N2}</td>");
                builder.AppendLine("</tr>");
            }

            builder.AppendLine("</table></body></html>");
            return builder.ToString();
        }

        private static string SanitizeFileName(string value)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            return new string(value
                .Select(ch => invalidChars.Contains(ch) ? '-' : ch)
                .ToArray())
                .Replace(' ', '-')
                .ToLowerInvariant();
        }

        private static string Encode(string? value) => WebUtility.HtmlEncode(value ?? string.Empty);
    }
}
