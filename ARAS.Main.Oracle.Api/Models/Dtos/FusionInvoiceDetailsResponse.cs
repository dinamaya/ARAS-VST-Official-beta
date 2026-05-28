using System.Text.Json.Serialization;

namespace ARAS.Main.Oracle.Api.Models.Dtos
{
    /// <summary>
    /// Envelope wrapper for the ARAS API response.
    /// Root structure: { "statusCode": 200, "message": "...", "data": [...] }
    /// </summary>
    public class FusionInvoiceApiEnvelope
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }

        // ---------------------------------------------------------------
        // FIXED: Was "ApiResponse" — actual JSON key from ARAS API is "data"
        // ---------------------------------------------------------------
        [JsonPropertyName("data")]
        public IEnumerable<FusionInvoiceDetailsResponse> ApiResponse { get; set; }
    }

    /// <summary>
    /// Represents a single invoice item from the ARAS API response.
    /// NOTE: All field names in the JSON are UPPER_CASE — mapped via [JsonPropertyName].
    /// </summary>
    public class FusionInvoiceDetailsResponse
    {
        [JsonPropertyName("INVOICEAMOUNT")]
        public double? InvoiceAmount { get; set; }

        [JsonPropertyName("INVOICEBALANCE")]
        public double? InvoiceBalance { get; set; }

        [JsonPropertyName("INVOICEDATE")]
        public DateTime InvoiceDate { get; set; }

        [JsonPropertyName("INVOICENUMBER")]
        public string InvoiceNumber { get; set; }

        [JsonPropertyName("CUSTOMERNAME")]
        public string CustomerName { get; set; }

        [JsonPropertyName("CUSTOMERNUMBER")]
        public string CustomerNumber { get; set; }

        [JsonPropertyName("DATASOURCE")]
        public string DataSource { get; set; }

        [JsonPropertyName("INVOICESTATUS")]
        public string InvoiceStatus { get; set; }
    }
}