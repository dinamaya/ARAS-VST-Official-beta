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

		[JsonPropertyName("INVOICEID")]
		public long? InvoiceId { get; set; }

		[JsonPropertyName("INVOICEBALANCE")]
		public double? InvoiceBalance { get; set; }

		[JsonPropertyName("INVOICEDATE")]
		public DateTime InvoiceDate { get; set; }

		[JsonPropertyName("INVOICENUMBER")]
		public string InvoiceNumber { get; set; }

		[JsonPropertyName("CUSTOMERNAME")]
		public string CustomerName { get; set; }

		// Some generic invoice-detail responses use the legacy CUSTNAME key.
		[JsonPropertyName("CUSTNAME")]
		public string CustomerNameLegacy { get; set; }

		[JsonPropertyName("CUSTOMERNUMBER")]
		public string CustomerNumber { get; set; }

		[JsonPropertyName("CUSTOMERCODE")]
		public string CustomerCode { get; set; }

		[JsonPropertyName("DATASOURCE")]
		public string DataSource { get; set; }

		[JsonPropertyName("INVOICESTATUS")]
		public string InvoiceStatus { get; set; }

		[JsonPropertyName("CUSTOMERTRXID")]
		public long? CustomerTrxId { get; set; }

		[JsonPropertyName("PAYMENTSCHEDULEID")]
		public long? PaymentScheduleId { get; set; }
		[JsonPropertyName("CURRENCYCODE")]
		public string? CurrencyCode { get; set; }

		[JsonPropertyName("GLACCOUNTINGDATE")]
		public DateTime? GlAccountingDate { get; set; }
	}

	public class FusionReceiptApiEnvelope
	{
		[JsonPropertyName("data")]
		public IEnumerable<FusionReceiptDetailsResponse> ApiResponse { get; set; }
	}

	/// <summary>
	/// Response returned by /aras/api/applicable-invoice-for-receipt.
	/// The gateway has used both compact upper-case field names and
	/// underscore-separated names, so the row model supports both contracts.
	/// </summary>
	public class FusionApplicableReceiptInvoiceApiEnvelope
	{
		public int StatusCode { get; set; }
		public string? Message { get; set; }

		[JsonPropertyName("data")]
		public IEnumerable<FusionApplicableReceiptInvoiceResponse>? Data { get; set; }
	}

	public class FusionApplicableReceiptInvoiceResponse
	{
		[JsonPropertyName("INVOICEID")] public long? InvoiceId { get; set; }
		[JsonPropertyName("INVOICE_ID")] public long? InvoiceIdUnderscore { get; set; }
		[JsonPropertyName("INVOICENUMBER")] public string? InvoiceNumber { get; set; }
		[JsonPropertyName("INVOICE_NUMBER")] public string? InvoiceNumberUnderscore { get; set; }
		[JsonPropertyName("INVOICEDATE")] public DateTime? InvoiceDate { get; set; }
		[JsonPropertyName("INVOICE_DATE")] public DateTime? InvoiceDateUnderscore { get; set; }
		[JsonPropertyName("DUEDATE")] public DateTime? DueDate { get; set; }
		[JsonPropertyName("DUE_DATE")] public DateTime? DueDateUnderscore { get; set; }
		[JsonPropertyName("CUSTOMERACCOUNTID")] public long? CustomerAccountId { get; set; }
		[JsonPropertyName("CUSTOMER_ACCOUNT_ID")] public long? CustomerAccountIdUnderscore { get; set; }
		[JsonPropertyName("CUSTOMERCODE")] public string? CustomerCode { get; set; }
		[JsonPropertyName("CUSTOMER_CODE")] public string? CustomerCodeUnderscore { get; set; }
		[JsonPropertyName("CUSTOMERNAME")] public string? CustomerName { get; set; }
		[JsonPropertyName("CUSTOMER_NAME")] public string? CustomerNameUnderscore { get; set; }
		[JsonPropertyName("INVOICEAMOUNT")] public double? InvoiceAmount { get; set; }
		[JsonPropertyName("INVOICE_AMOUNT")] public double? InvoiceAmountUnderscore { get; set; }
		[JsonPropertyName("INVOICEBALANCE")] public double? InvoiceBalance { get; set; }
		[JsonPropertyName("INVOICE_BALANCE")] public double? InvoiceBalanceUnderscore { get; set; }
		[JsonPropertyName("INVOICESTATUS")] public string? InvoiceStatus { get; set; }
		[JsonPropertyName("INVOICE_STATUS")] public string? InvoiceStatusUnderscore { get; set; }
		[JsonPropertyName("CURRENCYCODE")] public string? CurrencyCode { get; set; }
		[JsonPropertyName("CURRENCY_CODE")] public string? CurrencyCodeUnderscore { get; set; }
		[JsonPropertyName("PAYMENTSCHEDULEID")] public long? PaymentScheduleId { get; set; }
		[JsonPropertyName("PAYMENT_SCHEDULE_ID")] public long? PaymentScheduleIdUnderscore { get; set; }
		[JsonPropertyName("ORGID")] public long? OrgId { get; set; }
		[JsonPropertyName("ORG_ID")] public long? OrgIdUnderscore { get; set; }
	}

	public class FusionReceiptDetailsResponse
	{
		[JsonPropertyName("RECEIPTID")] public long? ReceiptId { get; set; }
		[JsonPropertyName("RECEIPTNUMBER")] public string ReceiptNumber { get; set; }
		[JsonPropertyName("RECEIPTDATE")] public DateTime? ReceiptDate { get; set; }
		[JsonPropertyName("RECEIPTAMOUNT")] public double? ReceiptAmount { get; set; }
		[JsonPropertyName("RECEIPTBALANCE")] public double? ReceiptBalance { get; set; }
		[JsonPropertyName("INVOICENUMBER")] public string InvoiceNumber { get; set; }
		[JsonPropertyName("INVOICE_NUMBER")] public string InvoiceNumberUnderscore { get; set; }
		[JsonPropertyName("TRXNUMBER")] public string TrxNumber { get; set; }
		[JsonPropertyName("TRX_NUMBER")] public string TrxNumberUnderscore { get; set; }
		[JsonPropertyName("AR_INVOICE_NUMBER")] public string ArInvoiceNumber { get; set; }
		[JsonPropertyName("INVOICEAMOUNT")] public double? InvoiceAmount { get; set; }
		[JsonPropertyName("INVOICEBALANCE")] public double? InvoiceBalance { get; set; }
		[JsonPropertyName("INVOICEDATE")] public DateTime InvoiceDate { get; set; }
		[JsonPropertyName("CUSTOMERNAME")] public string CustomerName { get; set; }
		[JsonPropertyName("CUSTOMERNUMBER")] public string CustomerNumber { get; set; }
		[JsonPropertyName("DATASOURCE")] public string DataSource { get; set; }
		[JsonPropertyName("CUSTOMERTRXID")] public long? CustomerTrxId { get; set; }
		[JsonPropertyName("PAYMENTSCHEDULEID")] public long? PaymentScheduleId { get; set; }
		[JsonPropertyName("CURRENCYCODE")] public string? CurrencyCode { get; set; }
		[JsonPropertyName("GLACCOUNTINGDATE")] public DateTime? GlAccountingDate { get; set; }
		[JsonPropertyName("creationDate")] public DateTime? CreationDate { get; set; }
	}
}
