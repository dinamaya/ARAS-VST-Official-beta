using ARAS.Main.Oracle.Api.App_Code.Globals.Constants;
using ARAS.Main.Oracle.Api.Context;
using ARAS.Main.Oracle.Api.Factories.Interfaces;
using ARAS.Main.Oracle.Api.Models.Dtos;
using ARAS.Main.Oracle.Api.Models.Entities;
using ARAS.Main.Oracle.Api.Repositories.Interfaces;
using ARAS.Main.Oracle.Api.Services.Interfaces;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;
using System.IO.Compression;
using System.Net.Http.Headers;
using System.Security;
using System.Text.Json;
using System.Text;

namespace ARAS.Main.Oracle.Api.Repositories.Implementations
{
	public class AdjustmentRepository : IAdjustmentRepository
	{
		private readonly MainDbContext efContext;
		private readonly IOracleConnectionFactory oracleConnection;
		private readonly IInvoiceRepository invoiceRepo;
		private readonly IConfigurationService _config;
		private readonly ILogger<IAdjustmentRepository> _logger;
		private readonly HttpClient _httpClient;
		private readonly HttpClient _fusionApiClient;

        public AdjustmentRepository(MainDbContext efContext, IOracleConnectionFactory oracleConnection, IConfigurationService config, ILogger<IAdjustmentRepository> logger, IInvoiceRepository invoiceRepo, IHttpClientFactory httpClientFactory)
        {
            this.efContext = efContext;
            this.oracleConnection = oracleConnection;
            _config = config;
            _logger = logger;
            this.invoiceRepo = invoiceRepo;
			_httpClient = httpClientFactory.CreateClient();
			_fusionApiClient = httpClientFactory.CreateClient("OracleFusionApi");
        }

        public async Task<IEnumerable<string>> GetReasonCodes()
		{
			if (_config.IsOntest())
				return _config.GetReasonCodes();

			var conn = await oracleConnection.OpenWithoutPolicyAsync();
			
			var sql = @"
				SELECT
					LV.LOOKUP_CODE
				FROM
					FND_LOOKUP_VALUES LV
				WHERE
					LV.VIEW_APPLICATION_ID = 222
					AND LV.SECURITY_GROUP_ID = 0
					AND lookup_type = 'ADJUST_REASON'
					AND enabled_flag = 'Y'
				ORDER BY
					LV.LOOKUP_CODE
			";

			var result = await conn.QueryAsync<string>(sql);
			var list = result.ToList();

			if(list == null || !list.Any())
				throw new InvalidOperationException(Exceptions.NULL_REASON_CODES);

			return list;
		}

		public async Task<IEnumerable<ReceivablesActivityDto>> GetReceivableActivities()
		{
			var conn = await oracleConnection.OpenWithoutPolicyAsync();

			var sql = @"
				SELECT   DISTINCT
					RECEIVABLES_TRX_ID Id,
					NAME Name
				FROM   ar_receivables_trx_all
				WHERE   TYPE = 'ADJUST'
			";

			var result = await conn.QueryAsync<ReceivablesActivityDto>(sql);
			return result;
		}

		public async Task Create(IEnumerable<AdjustmentPostingDto> data)
		{
			var rows = data
				.Where(item =>
					!string.Equals(item.AdjustmentActivity, "AP/AR Adjustment", StringComparison.OrdinalIgnoreCase) ||
					!string.IsNullOrWhiteSpace(item.ReasonCode) ||
					item.CustomerTrxId is not null)
				.GroupBy(item => item.HeaderId)
				.Select(group => group.First())
				.ToList();

			foreach(var item in rows)
			{
				if (string.Equals(item.AdjustmentActivity, "Bank Charge", StringComparison.OrdinalIgnoreCase))
				{
					await BackfillReceiptGlAccountingDateAsync(item);
					await CreateFusionReceiptActivityApplication(item);
					continue;
				}

				if (string.Equals(item.AdjustmentActivity, "Write-off Other Expense/Income", StringComparison.OrdinalIgnoreCase))
				{
					await CreateFusionSmallAmountWriteOff(item);
					continue;
				}

				if (item.CustomerTrxId is null)
					await BackfillCustomerTrxIdAsync(item);

				if (item.CustomerTrxId is null)
				{
					string documentType = IsReceiptAdjustment(item.AdjustmentActivity) ? "Receipt" : "Invoice";
					throw new InvalidOperationException($"CustomerTrxId is required for Fusion adjustment posting. {documentType}: {item.InvoiceNumber}");
				}

				await CreateFusionAdjustment(item);
			}
		}

		private async Task BackfillCustomerTrxIdAsync(AdjustmentPostingDto item)
		{
			if (string.IsNullOrWhiteSpace(item.InvoiceNumber))
				return;

			try
			{
				var requestUri = $"/aras/api/invoice/details?invoiceNumber={Uri.EscapeDataString(item.InvoiceNumber.Trim())}";
				using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
				var response = await _fusionApiClient.SendAsync(request);
				if (!response.IsSuccessStatusCode)
					return;

				var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
				var envelope = await response.Content.ReadFromJsonAsync<FusionInvoiceApiEnvelope>(options);
				var invoice = envelope?.ApiResponse?.FirstOrDefault(i =>
					string.Equals(i.InvoiceNumber, item.InvoiceNumber, StringComparison.OrdinalIgnoreCase));

				if (invoice is null)
					return;

				item.CustomerTrxId = invoice.CustomerTrxId ?? invoice.InvoiceId;
				item.PaymentScheduleId ??= invoice.PaymentScheduleId;
				item.CurrencyCode ??= invoice.CurrencyCode;
				item.GlAccountingDate ??= invoice.GlAccountingDate;
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, "CustomerTrxId lookup failed for invoice {InvoiceNumber}", item.InvoiceNumber);
			}
		}

		private async Task<FusionReceiptDetailsResponse?> TryGetReceiptDetailsAsync(string? receiptNumber)
		{
			if (string.IsNullOrWhiteSpace(receiptNumber))
				return null;

			try
			{
				var normalizedReceiptNumber = receiptNumber.Trim();
				var requestUri = $"/aras/api/receipt-details?receiptNumber={Uri.EscapeDataString(normalizedReceiptNumber)}";
				using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
				var response = await _fusionApiClient.SendAsync(request);

				if (!response.IsSuccessStatusCode)
					return null;

				var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
				var envelope = await response.Content.ReadFromJsonAsync<FusionReceiptApiEnvelope>(options);

				return envelope?.ApiResponse?.FirstOrDefault(r =>
					string.Equals(r.ReceiptNumber, normalizedReceiptNumber, StringComparison.OrdinalIgnoreCase));
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, "Receipt details lookup failed for receipt {ReceiptNumber}", receiptNumber);
				return null;
			}
		}

		private async Task BackfillReceiptGlAccountingDateAsync(AdjustmentPostingDto item)
		{
			var receipt = await TryGetReceiptDetailsAsync(item.InvoiceNumber);
			if (receipt is not null)
				item.GlAccountingDate = receipt.CreationDate ?? receipt.GlAccountingDate ?? item.GlAccountingDate;
		}

		private async Task CreateFusionSmallAmountWriteOff(AdjustmentPostingDto item)
		{
			var hasSelectedReceiptApplications = item.ReceiptApplications?.Any(a =>
				a is not null && a.PaymentScheduleId > 0 && a.AppliedAmount > 0) == true;

			// ApplyReceiptEnabled is a transport/control flag. The persisted application
			// rows are authoritative, so do not fall back to the legacy SOAP application
			// when selected invoices reached staging but the boolean was omitted by an
			// older client/API instance.
			if (!item.ApplyReceiptEnabled && hasSelectedReceiptApplications)
				item.ApplyReceiptEnabled = true;

			var receiptAmount = decimal.Round(Convert.ToDecimal(item.InvoiceAmount), 2, MidpointRounding.AwayFromZero);
			var writeOffAmount = decimal.Round(Convert.ToDecimal(item.AdjustmentAmount), 2, MidpointRounding.AwayFromZero);

			if (writeOffAmount <= 0)
				throw new InvalidOperationException($"The write-off amount must be greater than zero ({writeOffAmount:0.00}).");

			// Source detection:
			// - Receipt/GetReceiptDetails => use 2 calls and receipt activity name
			// - Invoice/GetInvoiceDetails => use 1 call and invoice activity name
			var receiptDetails = await TryGetReceiptDetailsAsync(item.InvoiceNumber);
			var isReceiptSource = receiptDetails is not null;

			if (isReceiptSource)
			{
				item.GlAccountingDate ??= receiptDetails!.CreationDate ?? receiptDetails.GlAccountingDate;

				if (receiptAmount <= 0)
					throw new InvalidOperationException($"The original receipt amount is missing or invalid ({receiptAmount:0.00}).");

				if (item.ApplyReceiptEnabled || hasSelectedReceiptApplications)
				{
					await ApplyReceiptToInvoicesAsync(item, receiptAmount);
				}
				else
				{
					if (writeOffAmount >= receiptAmount)
						throw new InvalidOperationException($"The write-off amount ({writeOffAmount:0.00}) must be less than the receipt amount ({receiptAmount:0.00}).");
				}
			}
			else
			{
				if (item.ApplyReceiptEnabled)
					throw new InvalidOperationException("Apply Receipt can only be used for receipt-based write-off transactions.");

				_logger.LogInformation(
					"SAR posting for {InvoiceNumber} detected as Invoice source. Using invoice-specific receivables activity.",
					item.InvoiceNumber);
			}

			// Receipt source -> Write-off Small Amount
			// Invoice source -> Write-Off Small Amount - INV
			var sarActivityName = isReceiptSource
				? "Write-off Small Amount"
				: "Write-Off Small Amount - INV";

			// Always call activity application
			await CreateFusionReceiptActivityApplication(item, writeOffAmount, sarActivityName);
		}

		private async Task ApplyReceiptToInvoicesAsync(AdjustmentPostingDto item, decimal receiptAmount)
		{
			_logger.LogInformation(
				"Apply Receipt staging payload: ReceiptNumber={ReceiptNumber}, ReceiptId={ReceiptId}, ApplyReceiptEnabled={ApplyReceiptEnabled}, Applications={ApplicationCount}",
				item.InvoiceNumber,
				item.ReceiptId,
				item.ApplyReceiptEnabled,
				item.ReceiptApplications?.Count ?? 0);

			if (item.ReceiptId is null || item.ReceiptId <= 0)
				throw new InvalidOperationException("ReceiptId is required to apply receipt to selected invoices.");

			var applications = item.ReceiptApplications?
				.Where(a => a is not null && a.PaymentScheduleId > 0 && a.AppliedAmount > 0)
				.ToList() ?? [];

			if (!applications.Any())
				throw new InvalidOperationException("Apply Receipt is enabled but no invoice applications were provided.");

			var applicableInvoices = (await invoiceRepo.GetApplicableInvoicesForReceipt(item.InvoiceNumber)).ToList();
			if (!applicableInvoices.Any())
				throw new InvalidOperationException($"No applicable invoices found for receipt {item.InvoiceNumber}.");

			var invoiceBySchedule = applicableInvoices.ToDictionary(a => a.PaymentScheduleId, a => a);
			var totalApplied = applications.Sum(a => decimal.Round(Convert.ToDecimal(a.AppliedAmount), 2, MidpointRounding.AwayFromZero));

			if (totalApplied > receiptAmount)
				throw new InvalidOperationException($"Total applied amount ({totalApplied:0.00}) cannot exceed receipt balance ({receiptAmount:0.00}).");

			foreach (var app in applications)
			{
				if (!invoiceBySchedule.TryGetValue(app.PaymentScheduleId, out var invoice))
					throw new InvalidOperationException($"Selected payment schedule {app.PaymentScheduleId} is no longer applicable for receipt {item.InvoiceNumber}.");

				var applyAmount = decimal.Round(Convert.ToDecimal(app.AppliedAmount), 2, MidpointRounding.AwayFromZero);
				var amountDue = decimal.Round(Convert.ToDecimal(invoice.InvoiceBalance), 2, MidpointRounding.AwayFromZero);
				if (applyAmount > amountDue)
					throw new InvalidOperationException($"Applied amount ({applyAmount:0.00}) for invoice {invoice.InvoiceNumber} exceeds current amount due ({amountDue:0.00}).");

				if (!string.IsNullOrWhiteSpace(app.CurrencyCode)
					&& !string.IsNullOrWhiteSpace(invoice.CurrencyCode)
					&& !string.Equals(app.CurrencyCode, invoice.CurrencyCode, StringComparison.OrdinalIgnoreCase))
				{
					throw new InvalidOperationException($"Currency mismatch for invoice {invoice.InvoiceNumber}: selected {app.CurrencyCode}, current {invoice.CurrencyCode}.");
				}

				await CreateFusionReceiptApplicationByRestAsync(item.ReceiptId.Value, app.PaymentScheduleId, applyAmount, invoice.InvoiceNumber);
			}
		}

		private async Task CreateFusionReceiptApplicationByRestAsync(long receiptId, long paymentScheduleId, decimal amountApplied, string invoiceNumber)
		{
			var endpointTemplate = _config.GetFusionReceiptApplyRestEndpointTemplate();
			var endpoint = endpointTemplate.Replace("{receiptId}", receiptId.ToString(), StringComparison.OrdinalIgnoreCase);

			using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
			request.Headers.Authorization = CreateBasicAuthenticationHeader();
			request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			request.Headers.TryAddWithoutValidation("REST-Framework-Version", "4");

			var payload = new
			{
				appliedPaymentScheduleId = paymentScheduleId,
				amountApplied = amountApplied
			};
			_logger.LogInformation(
				"Calling Fusion REST applyReceipt: ReceiptId={ReceiptId}, PaymentScheduleId={PaymentScheduleId}, AmountApplied={AmountApplied}, InvoiceNumber={InvoiceNumber}",
				receiptId,
				paymentScheduleId,
				amountApplied,
				invoiceNumber);

			request.Content = new StringContent(
				JsonSerializer.Serialize(payload),
				Encoding.UTF8,
				"application/vnd.oracle.adf.action+json");

			var response = await _httpClient.SendAsync(request);
			var content = await ReadResponseContent(response);
			_logger.LogInformation(
				"Fusion REST applyReceipt response: ReceiptId={ReceiptId}, PaymentScheduleId={PaymentScheduleId}, StatusCode={StatusCode}, Response={Response}",
				receiptId,
				paymentScheduleId,
				response.StatusCode,
				content);

			if (!response.IsSuccessStatusCode)
			{
				_logger.LogError("Fusion REST applyReceipt failed for invoice {InvoiceNumber}. StatusCode: {StatusCode}. Response: {Response}", invoiceNumber, response.StatusCode, content);
				throw new InvalidOperationException($"Fusion REST applyReceipt failed for invoice {invoiceNumber}. StatusCode: {(int)response.StatusCode}. Response: {content}");
			}

			if (!string.IsNullOrWhiteSpace(content) && content.Contains("errorCode", StringComparison.OrdinalIgnoreCase))
			{
				_logger.LogError("Fusion REST applyReceipt returned business error for invoice {InvoiceNumber}. Response: {Response}", invoiceNumber, content);
				throw new InvalidOperationException($"Fusion REST applyReceipt returned an error for invoice {invoiceNumber}. Response: {content}");
			}
		}

		private static bool IsReceiptAdjustment(string? adjustmentActivity) =>
			string.Equals(adjustmentActivity, "Cash Discount", StringComparison.OrdinalIgnoreCase) ||
			string.Equals(adjustmentActivity, "Bank Charge", StringComparison.OrdinalIgnoreCase) ||
			string.Equals(adjustmentActivity, "Write-off Other Expense/Income", StringComparison.OrdinalIgnoreCase);

		private async Task CreateFusionReceiptActivityApplication(AdjustmentPostingDto item, decimal? appliedAmount = null, string? activityName = null)
		{
			using var request = new HttpRequestMessage(
				HttpMethod.Post,
				_config.GetFusionReceiptActivityEndpoint());
			request.Headers.Authorization = CreateBasicAuthenticationHeader();
			request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/xml"));
			request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("identity"));
			request.Headers.TryAddWithoutValidation(
				"SOAPAction",
				$"\"{_config.GetFusionReceiptActivitySoapAction()}\"");
			request.Content = new StringContent(
				CreateFusionReceiptActivityApplicationEnvelope(item, appliedAmount ?? Convert.ToDecimal(item.AdjustmentAmount), activityName),
				Encoding.UTF8,
				"text/xml");

			var response = await _httpClient.SendAsync(request);
			var content = await ReadResponseContent(response);

			if (!response.IsSuccessStatusCode)
			{
				_logger.LogError(
					"Fusion receipt activity application failed. StatusCode: {StatusCode}. Response: {Response}",
					response.StatusCode,
					content);
				throw new InvalidOperationException(
					$"Fusion receipt activity application failed. StatusCode: {(int)response.StatusCode}. Response: {content}");
			}
		}

		private string CreateFusionReceiptActivityApplicationEnvelope(AdjustmentPostingDto item, decimal appliedAmount, string? activityName)
		{
			var applyDate = item.DateApplied.ToString("yyyy-MM-dd");
			var applyGlDate = (item.GlAccountingDate ?? item.InvoiceDate).ToString("yyyy-MM-dd");
			_logger.LogInformation(
				"Posting Fusion receipt activity for receipt {ReceiptNumber} with ApplyDate {ApplyDate} and ApplyGlDate {ApplyGlDate}",
				item.InvoiceNumber,
				applyDate,
				applyGlDate);
			var amount = appliedAmount.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
			var receivablesTrxId = _config.GetFusionAdjustmentReceivablesTrxId(activityName ?? item.AdjustmentActivity);
			var appliedPaymentScheduleId = _config.GetFusionReceiptActivityPaymentScheduleId();

			return $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/""
 xmlns:typ=""http://xmlns.oracle.com/apps/financials/receivables/receipts/shared/standardReceiptService/commonService/types/""
 xmlns:com=""http://xmlns.oracle.com/apps/financials/receivables/receipts/shared/standardReceiptService/commonService/"">
  <soapenv:Header/>
  <soapenv:Body>
    <typ:createActivityApplication>
      <typ:activityApplication>
        <com:BusinessUnit>{EscapeXml(_config.GetFusionReceiptActivityBusinessUnit())}</com:BusinessUnit>
        <com:ReceiptNumber>{EscapeXml(item.InvoiceNumber)}</com:ReceiptNumber>
        <com:ReceivablesTrxId>{receivablesTrxId}</com:ReceivablesTrxId>
        <com:AppliedPaymentScheduleId>{appliedPaymentScheduleId}</com:AppliedPaymentScheduleId>
        <com:AmountApplied>{amount}</com:AmountApplied>
        <com:ApplyDate>{applyDate}</com:ApplyDate>
        <com:ApplyGlDate>{applyGlDate}</com:ApplyGlDate>
      </typ:activityApplication>
    </typ:createActivityApplication>
  </soapenv:Body>
</soapenv:Envelope>";
		}

		private async Task CreateFusionAdjustment(AdjustmentPostingDto item)
		{
			using var request = new HttpRequestMessage(HttpMethod.Post, _config.GetFusionAdjustmentEndpoint());
			request.Headers.Authorization = CreateBasicAuthenticationHeader();
			request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/xml"));
			request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("identity"));
			request.Headers.TryAddWithoutValidation("SOAPAction", $"\"{_config.GetFusionAdjustmentSoapAction()}\"");
			request.Content = new StringContent(await CreateFusionAdjustmentEnvelope(item), Encoding.UTF8, "text/xml");

			var response = await _httpClient.SendAsync(request);
			var content = await ReadResponseContent(response);

			if (!response.IsSuccessStatusCode)
			{
				_logger.LogError("Fusion adjustment posting failed. StatusCode: {StatusCode}. Response: {Response}", response.StatusCode, content);
				throw new InvalidOperationException($"Fusion adjustment posting failed. StatusCode: {(int)response.StatusCode}. Response: {content}");
			}
		}

		private static async Task<string> ReadResponseContent(HttpResponseMessage response)
		{
			var bytes = await response.Content.ReadAsByteArrayAsync();
			var encoding = response.Content.Headers.ContentEncoding.FirstOrDefault();

			if (string.Equals(encoding, "gzip", StringComparison.OrdinalIgnoreCase))
				return await Decompress(bytes, stream => new GZipStream(stream, CompressionMode.Decompress));

			if (string.Equals(encoding, "deflate", StringComparison.OrdinalIgnoreCase))
				return await Decompress(bytes, stream => new DeflateStream(stream, CompressionMode.Decompress));

			if (string.Equals(encoding, "br", StringComparison.OrdinalIgnoreCase))
				return await Decompress(bytes, stream => new BrotliStream(stream, CompressionMode.Decompress));

			return Encoding.UTF8.GetString(bytes);
		}

		private static async Task<string> Decompress(byte[] bytes, Func<Stream, Stream> createStream)
		{
			await using var compressed = new MemoryStream(bytes);
			await using var decompressed = createStream(compressed);
			using var reader = new StreamReader(decompressed, Encoding.UTF8);

			return await reader.ReadToEndAsync();
		}

		private AuthenticationHeaderValue CreateBasicAuthenticationHeader()
		{
			var credentials = $"{_config.GetFusionAdjustmentUsername()}:{_config.GetFusionAdjustmentPassword()}";
			var bytes = Encoding.UTF8.GetBytes(credentials);

			return new AuthenticationHeaderValue("Basic", Convert.ToBase64String(bytes));
		}

		private async Task<string> CreateFusionAdjustmentEnvelope(AdjustmentPostingDto item)
		{
			var applyDate = item.DateApplied.ToString("yyyy-MM-dd");
			var adjustmentAmount = decimal.Round(Convert.ToDecimal(item.AdjustmentAmount), 2, MidpointRounding.AwayFromZero);
			var invoiceAmount = decimal.Round(Convert.ToDecimal(item.InvoiceAmount), 2, MidpointRounding.AwayFromZero);
			var amount = adjustmentAmount.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
			var currencyCode = EscapeXml(string.IsNullOrWhiteSpace(item.CurrencyCode) ? "USD" : item.CurrencyCode.Trim().ToUpperInvariant());
			var isAparFlow = string.Equals(item.AdjustmentActivity, "AP/AR Adjustment", StringComparison.OrdinalIgnoreCase);
			var isAparWriteOffFlow = string.Equals(item.AdjustmentActivity, "Write-Off Small Amount - INV", StringComparison.OrdinalIgnoreCase);
			var glDateValue = isAparFlow
				? item.DateApplied.Date
				: ResolveFusionGlDate(item);
			var glDate = glDateValue.ToString("yyyy-MM-dd");
			var transactionAdjustmentType = (isAparFlow || isAparWriteOffFlow) ? "LINE" : (adjustmentAmount >= invoiceAmount ? "INVOICE" : "LINE");
			var receivablesTrxId = _config.GetFusionAdjustmentReceivablesTrxId(item.AdjustmentActivity);
			var referenceData = await GetAdjustmentReferenceData(item.CustomerTrxId!.Value);
			var paymentScheduleId = item.PaymentScheduleId ?? referenceData?.PaymentScheduleId;
			var orgId = referenceData?.OrgId ?? _config.GetFusionAdjustmentOrgId();
			var setOfBooksId = referenceData?.SetOfBooksId ?? _config.GetFusionAdjustmentSetOfBooksId();

			return $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/""
                  xmlns:typ=""http://xmlns.oracle.com/apps/financials/receivables/transactions/adjustments/adjustmentService/types/""
                  xmlns:rec=""http://xmlns.oracle.com/apps/financials/receivables/transactions/adjustments/adjustmentService/"">
  <soapenv:Header/>
  <soapenv:Body>
    <typ:createAdjustment>
      <typ:adjustment>
        <rec:CustomerTrxId>{item.CustomerTrxId}</rec:CustomerTrxId>
        {CreateOptionalPaymentScheduleId(paymentScheduleId)}
        <rec:Amount currencyCode=""{currencyCode}"">{amount}</rec:Amount>
        <rec:ApplyDate>{applyDate}</rec:ApplyDate>
        <rec:GlDate>{glDate}</rec:GlDate>
        <rec:ReasonCode>{EscapeXml(item.ReasonCode)}</rec:ReasonCode>
        <rec:TransactionAdjustmentType>{transactionAdjustmentType}</rec:TransactionAdjustmentType>
        <rec:ReceivablesTrxId>{receivablesTrxId}</rec:ReceivablesTrxId>
        <rec:CodeCombinationId>{_config.GetFusionAdjustmentCodeCombinationId(item.AdjustmentActivity)}</rec:CodeCombinationId>
        <rec:SetOfBooksId>{setOfBooksId}</rec:SetOfBooksId>
        <rec:OrgId>{orgId}</rec:OrgId>
        <rec:Comments>{EscapeXml(item.Remarks)}</rec:Comments>
        <rec:AdjustmentType>M</rec:AdjustmentType>
      </typ:adjustment>
    </typ:createAdjustment>
  </soapenv:Body>
</soapenv:Envelope>";
		}

		private static string CreateOptionalPaymentScheduleId(long? paymentScheduleId) =>
			paymentScheduleId.HasValue ? $"<rec:PaymentScheduleId>{paymentScheduleId.Value}</rec:PaymentScheduleId>" : string.Empty;

		private static DateTime ResolveFusionGlDate(AdjustmentPostingDto item)
		{
			if (IsValidFusionDate(item.GlAccountingDate))
				return item.GlAccountingDate!.Value.Date;

			if (IsValidFusionDate(item.InvoiceDate))
				return item.InvoiceDate.Date;

			return item.DateApplied.Date;
		}

		private static bool IsValidFusionDate(DateTime? date) =>
			date.HasValue && date.Value.Year > 1900;

		private static string EscapeXml(string? value) => SecurityElement.Escape(value ?? string.Empty) ?? string.Empty;

		private async Task<AdjustmentReferenceDataItem?> GetAdjustmentReferenceData(long customerTrxId)
		{
			var url = $"/aras/api/adjustment-reference-data?customerTrxId={customerTrxId}";
			using var request = new HttpRequestMessage(HttpMethod.Get, url);

			var response = await _fusionApiClient.SendAsync(request);
			var content = await ReadResponseContent(response);

			if (!response.IsSuccessStatusCode)
			{
				_logger.LogWarning("Adjustment reference data lookup failed for CustomerTrxId {CustomerTrxId}. StatusCode: {StatusCode}. Response: {Response}", customerTrxId, response.StatusCode, content);
				return null;
			}

			var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
			var payload = JsonSerializer.Deserialize<AdjustmentReferenceDataResponse>(content, options);

			return payload?.Data?.FirstOrDefault();
		}

        public async Task<ReceivablesActivityDto> GetReceivableActivityByName(string adjustmentActivity)
        {
			var conn = await oracleConnection.OpenWithoutPolicyAsync();

			var sql = @"
				SELECT DISTINCT
					RECEIVABLES_TRX_ID Id,
					NAME Name
				FROM
					ar_receivables_trx_all
				WHERE
					TYPE = 'ADJUST' AND 
					UPPER(TRIM(NAME)) = UPPER(:adjustmentActivity) 
			";

			var result = await conn.QueryFirstOrDefaultAsync<ReceivablesActivityDto>(sql,
				new { adjustmentActivity = $"{adjustmentActivity}" },
				commandTimeout: 120
			) ?? throw new Exception(Exceptions.INVALID_RECEIVABLE_ACTIVITY);
			return result;
		}

        public async Task<ReceivablesActivityDto> GetReceivableActivityByName(OracleConnection oracleConnection, string adjustmentActivity)
        {
			var sql = @"
				SELECT DISTINCT
					RECEIVABLES_TRX_ID Id,
					NAME Name
				FROM
					ar_receivables_trx_all
				WHERE
					TYPE = 'ADJUST' AND 
					UPPER(TRIM(NAME)) = UPPER(:adjustmentActivity) 
			";

			var result = await oracleConnection.QueryFirstOrDefaultAsync<ReceivablesActivityDto>(sql,
				new { adjustmentActivity = $"{adjustmentActivity}" },
				commandTimeout: 120
			) ?? throw new Exception(Exceptions.INVALID_RECEIVABLE_ACTIVITY);
			return result;
		}

		public async Task<IEnumerable<ARAdjustmentsStaging>> GetAll() => await efContext.AdjustmentsStaging.ToListAsync();

		private async Task CreateStageRow(IEnumerable<AdjustmentCreateStagingRow> rows)
		{
			var list = rows.Select(row => new ARAdjustmentsStaging
			{
                HeaderId = row.AdjustmentsDetails.HeaderId,
                CustomerTrxId = long.Parse(row.CustomerTrxId),
				InvoiceNumber = row.AdjustmentsDetails.InvoiceNumber,
				Amount = Convert.ToDecimal(row.AdjustmentsDetails.AdjustmentAmount),
				CreatedFrom = "ADJUSTMENT API",
				GlDate = row.AdjustmentsDetails.InvoiceDate,
				Type = "LINE",
				PaymentScheduleId = null,
				ApplyDate = row.AdjustmentsDetails.DateApplied,
				ReceivablesTrxId = row.ReceivableActivityId,
				ReasonCode = row.AdjustmentsDetails.ReasonCode,
				Comments = row.AdjustmentsDetails.Remarks,
				AccountName = row.AdjustmentsDetails.CustomerName,
				AccountNumber = long.Parse(row.AdjustmentsDetails.CustomerNumber),
				StgFlag = "1",
				IntFlag = "0",
				InvFlag = "0"
			})
			.GroupBy(x => x.HeaderId)
			.Select(g => g.First())
			.ToList();

			var headerIds = list.Select(x => x.HeaderId).ToList();

			var existing = await efContext.AdjustmentsStaging
				.Where(x => headerIds.Contains(x.HeaderId))
				.Select(x => x.HeaderId)
				.ToListAsync();

			var existingSet = existing.ToHashSet();

			list = list
				.Where(x => !existingSet.Contains(x.HeaderId))
				.ToList();

			if (!list.Any())
				return;

			const string sql = @"
				INSERT INTO APPS.XXMSI_AR_ADJ_STG
				(
					HEADER_ID,
					CUSTOMER_TRX_ID,
					INVOICE_NUMBER,
					AMOUNT,
					CREATED_FROM,
					GL_DATE,
					TYPE,
					PAYMENT_SCHEDULE_ID,
					APPLY_DATE,
					RECEIVABLES_TRX_ID,
					REASON_CODE,
					COMMENTS,
					ACCOUNT_NAME,
					ACCOUNT_NUMBER,
					STG_FLAG,
					INT_FLAG,
					INV_FLAG
				)
				VALUES
				(
					:HeaderId,
					:CustomerTrxId,
					:InvoiceNumber,
					:Amount,
					:CreatedFrom,
					:GlDate,
					:Type,
					:PaymentScheduleId,
					:ApplyDate,
					:ReceivablesTrxId,
					:ReasonCode,
					:Comments,
					:AccountName,
					:AccountNumber,
					:StgFlag,
					:IntFlag,
					:InvFlag
				)";

			var insertConn = await oracleConnection.OpenWithoutPolicyAsync();
			await insertConn.ExecuteAsync(sql, list, commandTimeout: 120);
		}

		public async Task<IEnumerable<PostedResponseDto>> GetPosted(IEnumerable<long> adjustmentIds)
        {
			var headerIds = adjustmentIds.Distinct().ToList();
			if (!headerIds.Any()) return [];

			var conn = await oracleConnection.OpenWithoutPolicyAsync();
			var parameters = new DynamicParameters();
			var headerPredicates = new List<string>();

			for (int i = 0; i < headerIds.Count; i++)
			{
				var parameterName = $"headerId{i}";
				headerPredicates.Add($"stg.HEADER_ID = :{parameterName}");
				parameters.Add(parameterName, headerIds[i]);
			}

			string sql = $@"
				SELECT
					stg.HEADER_ID HeaderId,
					CAST(NULL AS VARCHAR2(240)) AdjustmentActivityName
				FROM
					APPS.XXMSI_AR_ADJ_STG stg
				WHERE
					({string.Join(" OR ", headerPredicates)}) AND
					stg.STG_FLAG = '1' AND
					stg.INT_FLAG = '1' AND
					stg.INV_FLAG = '1'";

			return (await conn.QueryAsync<PostedResponseDto>(sql, parameters, commandTimeout: 120)).ToList();
		}
    }
}
