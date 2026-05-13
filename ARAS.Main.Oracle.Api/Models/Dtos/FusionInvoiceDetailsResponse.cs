using System.Text.Json.Serialization;

namespace ARAS.Main.Oracle.Api.Models.Dtos
{
    /// <summary>
    /// Envelope wrapper for the Oracle Fusion Integration Hub API response.
    /// Endpoint: GET /fin/api/masterdata/financials/invoice-details
    /// Root structure: { "statusCode": 200, "message": "...", "apiResponse": [...] }
    /// </summary>
    public class FusionInvoiceApiEnvelope
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }

        // The array is under "apiResponse" — not "data"
        public IEnumerable<FusionInvoiceDetailsResponse> ApiResponse { get; set; }
    }

    /// <summary>
    /// Represents a single invoice item from the Oracle Fusion Integration Hub API.
    /// NOTE: The API has a typo — "invoiceAmmount" (double 'm').
    ///       JsonPropertyName is used to map it correctly without breaking our model.
    /// NOTE: invoiceAmmount and invoiceBalance are nullable in the API response.
    /// </summary>
    public class FusionInvoiceDetailsResponse
    {
        // API returns "invoiceAmmount" (typo with double 'm') — mapped explicitly
        [JsonPropertyName("invoiceAmmount")]
        public double? InvoiceAmount { get; set; }

        public double? InvoiceBalance { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string InvoiceNumber { get; set; }
        public string CustomerName { get; set; }
        public string CustomerNumber { get; set; }
        public string DataSource { get; set; }
        public string InvoiceStatus { get; set; }
    }
}