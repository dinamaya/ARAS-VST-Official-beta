using ARAS.Blazor.Models.DTOs;
using ARAS.Blazor.Services.Interfaces;
using Microsoft.JSInterop;
using System.Net;
using System.Text;
using System.IO.Compression;
using System.Xml;

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
            ArgumentNullException.ThrowIfNull(filters);

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
            ArgumentNullException.ThrowIfNull(rows);
            ArgumentNullException.ThrowIfNull(filters);

            var bytes = BuildExcelWorkbook(rows, filters);
            var fileName = $"reports-{SanitizeFileName(filters.ReportType)}-{DateTime.Now:yyyyMMddHHmmss}.xlsx";
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

        private static byte[] BuildExcelWorkbook(IEnumerable<ReportsDto> rows, ReportFiltersDto filters)
        {
            using var stream = new MemoryStream();
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
            {
                WriteZipEntry(archive, "[Content_Types].xml", BuildContentTypesXml());
                WriteZipEntry(archive, "_rels/.rels", BuildRootRelationshipsXml());
                WriteZipEntry(archive, "xl/workbook.xml", BuildWorkbookXml());
                WriteZipEntry(archive, "xl/_rels/workbook.xml.rels", BuildWorkbookRelationshipsXml());
                WriteZipEntry(archive, "xl/styles.xml", BuildStylesXml());
                WriteZipEntry(archive, "xl/worksheets/sheet1.xml", BuildWorksheetXml(rows, filters));
            }

            return stream.ToArray();
        }

        private static void WriteZipEntry(ZipArchive archive, string entryName, string content)
        {
            var entry = archive.CreateEntry(entryName, CompressionLevel.Fastest);
            using var entryStream = entry.Open();
            using var writer = new StreamWriter(entryStream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            writer.Write(content);
        }

        private static string BuildContentTypesXml() =>
            """
            <?xml version="1.0" encoding="utf-8"?>
            <Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
              <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml" />
              <Default Extension="xml" ContentType="application/xml" />
              <Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml" />
              <Override PartName="/xl/styles.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml" />
              <Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml" />
            </Types>
            """;

        private static string BuildRootRelationshipsXml() =>
            """
            <?xml version="1.0" encoding="utf-8"?>
            <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
              <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml" />
            </Relationships>
            """;

        private static string BuildWorkbookXml() =>
            """
            <?xml version="1.0" encoding="utf-8"?>
            <workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"
                      xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
              <sheets>
                <sheet name="Reports" sheetId="1" r:id="rId1" />
              </sheets>
            </workbook>
            """;

        private static string BuildWorkbookRelationshipsXml() =>
            """
            <?xml version="1.0" encoding="utf-8"?>
            <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
              <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml" />
              <Relationship Id="rId2" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles" Target="styles.xml" />
            </Relationships>
            """;

        private static string BuildStylesXml() =>
            """
            <?xml version="1.0" encoding="utf-8"?>
            <styleSheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
              <fonts count="1">
                <font>
                  <sz val="11" />
                  <name val="Calibri" />
                </font>
              </fonts>
              <fills count="2">
                <fill><patternFill patternType="none" /></fill>
                <fill><patternFill patternType="gray125" /></fill>
              </fills>
              <borders count="2">
                <border>
                  <left /><right /><top /><bottom /><diagonal />
                </border>
                <border>
                  <left style="thin"><color auto="1" /></left>
                  <right style="thin"><color auto="1" /></right>
                  <top style="thin"><color auto="1" /></top>
                  <bottom style="thin"><color auto="1" /></bottom>
                  <diagonal />
                </border>
              </borders>
              <cellStyleXfs count="1">
                <xf numFmtId="0" fontId="0" fillId="0" borderId="0" />
              </cellStyleXfs>
              <cellXfs count="2">
                <xf numFmtId="0" fontId="0" fillId="0" borderId="0" xfId="0" />
                <xf numFmtId="0" fontId="0" fillId="0" borderId="1" xfId="0" applyBorder="1" />
              </cellXfs>
              <cellStyles count="1">
                <cellStyle name="Normal" xfId="0" builtinId="0" />
              </cellStyles>
            </styleSheet>
            """;

        private static string BuildWorksheetXml(IEnumerable<ReportsDto> rows, ReportFiltersDto filters)
        {
            using var stream = new MemoryStream();
            var settings = new XmlWriterSettings
            {
                Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
                OmitXmlDeclaration = false
            };

            using (var writer = XmlWriter.Create(stream, settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("worksheet", "http://schemas.openxmlformats.org/spreadsheetml/2006/main");
                writer.WriteStartElement("sheetViews");
                writer.WriteStartElement("sheetView");
                writer.WriteAttributeString("workbookViewId", "0");
                writer.WriteAttributeString("showGridLines", "1");
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteStartElement("sheetData");

                var rowIndex = 1u;
                WriteRow(writer, rowIndex++, ["Report Type", filters.ReportType]);
                WriteRow(writer, rowIndex++, ["Date Range", $"{filters.StartDate:yyyy-MM-dd} to {filters.EndDate:yyyy-MM-dd}"]);
                WriteRow(writer, rowIndex++, ["Status", string.IsNullOrWhiteSpace(filters.Status) ? "All Statuses" : filters.Status]);
                WriteRow(writer, rowIndex++, ["Adjustment Type", string.IsNullOrWhiteSpace(filters.AdjustmentType) ? "All Adjustment Types" : filters.AdjustmentType]);
                WriteRow(writer, rowIndex++, ["Customer", string.IsNullOrWhiteSpace(filters.CustomerName) ? "All Customers" : filters.CustomerName]);
                WriteRow(writer, rowIndex++, ["Requestor", string.IsNullOrWhiteSpace(filters.RequestorName) ? "All Requestors" : filters.RequestorName]);
                WriteEmptyRow(writer, rowIndex++);

                WriteRow(writer, rowIndex++, [
                    "Request No",
                    "Category",
                    "Customer",
                    "Invoice No",
                    "Adjustment Type",
                    "Requestor",
                    "Approver",
                    "Status",
                    "Date Requested",
                    "Date Approved",
                    "Date Updated",
                    "Updated By",
                    "Amount"
                ]);

                foreach (var row in rows)
                {
                    WriteRow(writer, rowIndex++, [
                        row.RequestNumber,
                        row.Category,
                        row.CustomerName,
                        row.InvoiceNumber,
                        row.AdjustmentType,
                        row.Requestor,
                        row.Approver,
                        row.Status,
                        row.DateRequested.ToString("yyyy-MM-dd HH:mm"),
                        row.DateApproved?.ToString("yyyy-MM-dd HH:mm") ?? string.Empty,
                        row.DateUpdated.ToString("yyyy-MM-dd HH:mm"),
                        row.UpdatedBy,
                        row.Amount
                    ]);
                }

                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }

            return Encoding.UTF8.GetString(stream.ToArray());
        }

        private static void WriteRow(XmlWriter writer, uint rowIndex, IEnumerable<object?> values)
        {
            writer.WriteStartElement("row");
            writer.WriteAttributeString("r", rowIndex.ToString());

            var columnIndex = 1;
            foreach (var value in values)
            {
                WriteCell(writer, columnIndex++, rowIndex, value);
            }

            writer.WriteEndElement();
        }

        private static void WriteEmptyRow(XmlWriter writer, uint rowIndex)
        {
            writer.WriteStartElement("row");
            writer.WriteAttributeString("r", rowIndex.ToString());
            writer.WriteEndElement();
        }

        private static void WriteCell(XmlWriter writer, int columnIndex, uint rowIndex, object? value)
        {
            writer.WriteStartElement("c");
            writer.WriteAttributeString("r", $"{GetColumnName(columnIndex)}{rowIndex}");
            writer.WriteAttributeString("s", "1");

            switch (value)
            {
                case null:
                    break;
                case byte or sbyte or short or ushort or int or uint or long or ulong or float or double or decimal:
                    writer.WriteElementString("v", Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture));
                    break;
                default:
                    writer.WriteAttributeString("t", "inlineStr");
                    writer.WriteStartElement("is");
                    writer.WriteElementString("t", value.ToString() ?? string.Empty);
                    writer.WriteEndElement();
                    break;
            }

            writer.WriteEndElement();
        }

        private static string GetColumnName(int columnIndex)
        {
            var dividend = columnIndex;
            var columnName = string.Empty;

            while (dividend > 0)
            {
                var modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar('A' + modulo) + columnName;
                dividend = (dividend - modulo) / 26;
            }

            return columnName;
        }

        private static string SanitizeFileName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "report";

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
