using ARAS.Main.Oracle.Api.App_Code.Globals.Constants;
using ARAS.Main.Oracle.Api.Context;
using ARAS.Main.Oracle.Api.Factories.Interfaces;
using ARAS.Main.Oracle.Api.Models.Dtos;
using ARAS.Main.Oracle.Api.Repositories.Interfaces;
using ARAS.Main.Oracle.Api.Services.Interfaces;
using Dapper;
using Oracle.ManagedDataAccess.Client;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace ARAS.Main.Oracle.Api.Repositories.Implementations
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly MainDbContext efContext;
        private readonly IOracleConnectionFactory oracleConnection;
        private readonly IConfigurationService _config;

        // -------------------------------------------------------------------------
        // Oracle Fusion Migration:
        // HttpClient is injected and configured via the named client "OracleFusionApi".
        // This replaces the direct Oracle EBS database connection for invoice lookups.
        // -------------------------------------------------------------------------
        private readonly HttpClient _fusionHttpClient;

		public InvoiceRepository(
            MainDbContext efContext,
            IOracleConnectionFactory oracleConnection,
            IConfigurationService config,
            IHttpClientFactory httpClientFactory)
        {
            this.efContext = efContext;
            this.oracleConnection = oracleConnection;
            _config = config;
            _fusionHttpClient = httpClientFactory.CreateClient("OracleFusionApi");
        }

        private async Task<IEnumerable<InvoiceDetailsDto>> GetFusionApInvoiceDetailsAsync(string? invoiceNumber = null, string? customerName = null, string? customerNumber = null)
        {
            var queryParams = new List<string>();

            if (!string.IsNullOrWhiteSpace(invoiceNumber))
                queryParams.Add($"trxNumber={Uri.EscapeDataString(invoiceNumber.Trim())}");

            if (!string.IsNullOrWhiteSpace(customerName))
                queryParams.Add($"customerName={Uri.EscapeDataString(customerName.Trim())}");
            if (!string.IsNullOrWhiteSpace(customerNumber))
                queryParams.Add($"customerNumber={Uri.EscapeDataString(customerNumber.Trim())}");

            var requestUri = "/aras/api/ap/invoice/details"
                + (queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : string.Empty);

            var response = await _fusionHttpClient.GetAsync(requestUri);
            response.EnsureSuccessStatusCode();

            var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var envelope = await response.Content.ReadFromJsonAsync<FusionInvoiceApiEnvelope>(jsonOptions)
                ?? throw new InvalidOperationException(Exceptions.NULL_INVOICE_DETAILS);

            var fusionResult = envelope.ApiResponse
                ?? throw new InvalidOperationException(Exceptions.NULL_INVOICE_DETAILS);

            var mapped = fusionResult.Select(f => new InvoiceDetailsDto
            {
                Id = f.InvoiceNumber,
                InvoiceNumber = f.InvoiceNumber,
                InvoiceAmount = f.InvoiceAmount ?? 0,
                InvoiceDate = f.InvoiceDate,
                CustomerName = f.CustomerName ?? f.CustomerNameLegacy,
                CustomerNumber = f.CustomerNumber,
                DataSource = f.DataSource ?? "AP",
                InvoiceBalance = f.InvoiceBalance ?? 0,
                CustomerTrxId = f.CustomerTrxId,
                PaymentScheduleId = f.PaymentScheduleId
            }).ToList();

            await BackfillPaymentScheduleIdsAsync(mapped);

            return mapped;
        }
        private async Task<IEnumerable<InvoiceDetailsDto>> GetFusionReceiptDetailsAsync(string? receiptNumber = null, string? customerName = null, string? customerNumber = null)
        {
            var queryParams = new List<string>();
            if (!string.IsNullOrWhiteSpace(receiptNumber))
                queryParams.Add($"receiptNumber={Uri.EscapeDataString(receiptNumber.Trim())}");
            if (!string.IsNullOrWhiteSpace(customerName))
                queryParams.Add($"customerName={Uri.EscapeDataString(customerName.Trim())}");
            if (!string.IsNullOrWhiteSpace(customerNumber))
                queryParams.Add($"customerNumber={Uri.EscapeDataString(customerNumber.Trim())}");

            var requestUri = "/aras/api/receipt-details"
                + (queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : string.Empty);
            var response = await _fusionHttpClient.GetAsync(requestUri);
            response.EnsureSuccessStatusCode();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var envelope = await response.Content.ReadFromJsonAsync<FusionReceiptApiEnvelope>(options)
                ?? throw new InvalidOperationException(Exceptions.NULL_INVOICE_DETAILS);

            var mapped = (envelope.ApiResponse ?? []).Select(r =>
            {
                var invoiceNumber = r.InvoiceNumber
                    ?? r.InvoiceNumberUnderscore
                    ?? r.TrxNumber
                    ?? r.TrxNumberUnderscore
                    ?? r.ArInvoiceNumber;

                return new InvoiceDetailsDto
                {
                    Id = invoiceNumber ?? r.ReceiptNumber,
                    InvoiceNumber = invoiceNumber,
                    InvoiceAmount = r.InvoiceAmount ?? 0,
                    InvoiceBalance = r.InvoiceBalance ?? 0,
                    InvoiceDate = r.InvoiceDate,
                    CustomerName = r.CustomerName,
                    CustomerNumber = r.CustomerNumber,
                    DataSource = r.DataSource ?? "AR",
                    CustomerTrxId = r.CustomerTrxId,
                    PaymentScheduleId = r.PaymentScheduleId,
                    ReceiptNumber = r.ReceiptNumber,
                    ReceiptDate = r.ReceiptDate,
                    ReceiptAmount = r.ReceiptAmount,
                    ReceiptBalance = r.ReceiptBalance,
                    CurrencyCode = r.CurrencyCode,
                    GlAccountingDate = r.GlAccountingDate
                };
            }).ToList();

            await BackfillReceiptCustomerTrxIdsAsync(mapped);
            return mapped;
        }

        private async Task BackfillReceiptCustomerTrxIdsAsync(IEnumerable<InvoiceDetailsDto> receipts)
        {
            foreach (var receipt in receipts.Where(r =>
                r.CustomerTrxId is null && !string.IsNullOrWhiteSpace(r.InvoiceNumber)))
            {
                try
                {
                    var requestUri = $"/aras/api/invoice/details?trxNumber={Uri.EscapeDataString(receipt.InvoiceNumber.Trim())}";
                    var response = await _fusionHttpClient.GetAsync(requestUri);
                    if (!response.IsSuccessStatusCode)
                        continue;

                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var envelope = await response.Content.ReadFromJsonAsync<FusionInvoiceApiEnvelope>(options);
                    var invoice = envelope?.ApiResponse?.FirstOrDefault(i =>
                        string.Equals(i.InvoiceNumber, receipt.InvoiceNumber, StringComparison.OrdinalIgnoreCase));

                    if (invoice?.CustomerTrxId is long customerTrxId)
                    {
                        receipt.CustomerTrxId = customerTrxId;
                        receipt.PaymentScheduleId ??= invoice.PaymentScheduleId;
                    }
                }
                catch (Exception ex)
                {
                    // Keep the search result available. Posting will provide the
                    // explicit missing-transaction error if the fallback fails.
                    System.Diagnostics.Debug.WriteLine(
                        $"Receipt CustomerTrxId lookup failed for {receipt.InvoiceNumber}: {ex.Message}");
                }
            }
        }

        public async Task<IEnumerable<InvoiceDetailsDto>> GetReceiptsDetails(SearchRequestDto searchRequest)
        {
            var category = searchRequest.Category?.Trim();
            var value = searchRequest.Value?.Trim();

            if (category is not "Receipt Number"
                and not "Receipt Customer Name"
                and not "Receipt Customer Number")
            {
                throw new ArgumentException(
                    "Invalid receipt search category. Use Receipt Number, Receipt Customer Name, or Receipt Customer Number.");
            }

            var rows = await GetFusionReceiptDetailsAsync(
                receiptNumber: category == "Receipt Number" ? value : null,
                customerName: category == "Receipt Customer Name" ? value : null,
                customerNumber: category == "Receipt Customer Number" ? value : null);

            var min = searchRequest.StartDate.ToDateTime(TimeOnly.MinValue);
            var max = searchRequest.EndDate.ToDateTime(TimeOnly.MaxValue);

            return rows.Where(r =>
                !r.ReceiptDate.HasValue ||
                (min <= r.ReceiptDate.Value && r.ReceiptDate.Value <= max));
        }

        // =========================================================================
        // GetInvoiceDetails — MIGRATED TO ORACLE FUSION (ARAS API)
        // Previously: /fin/api/masterdata/financials/invoice-details
        // Now:        GET /aras/api/invoice/details?trxNumber&customerName
        // BaseUrl:    https://data-model-gateway-uat.msi-ecs.com.ph
        // =========================================================================
        public async Task<IEnumerable<InvoiceDetailsDto>> GetInvoiceDetails(SearchRequestDto searchRequest)
        {
            searchRequest.Category = searchRequest.Category.Trim();
            searchRequest.Value = searchRequest.Value.Trim();

            DateTime min = searchRequest.StartDate.ToDateTime(TimeOnly.MinValue);
            DateTime max = searchRequest.EndDate.ToDateTime(TimeOnly.MaxValue);

            if (_config.IsOntest())
            {
                var list = Enumerable.Range(1, 100)
                    .Select(i => new InvoiceDetailsDto
                    {
                        Id = $"INV-{i:000}",
                        InvoiceNumber = $"5{i:00000}",
                        InvoiceAmount = 10000 + (i * 50),
                        InvoiceDate = new DateTime(2024, 1, 1).AddDays(i),
                        CustomerName = $"Customer {i}",
                        CustomerNumber = $"CUST-{1000 + i}",
                        DataSource = "AR",
                        InvoiceBalance = i % 5 == 0 ? 0 : i * 100
                    });

                if (searchRequest.Category is "Invoice Customer Name" or "Customer Name")
                    return list.Where(l => l.CustomerName == searchRequest.Value);
                if (searchRequest.Category == "Invoice Customer Number")
                    return list.Where(l => l.CustomerNumber == searchRequest.Value);

                throw new Exception("Invalid Search Category. Please provide correct search category (Invoice Number or Customer Name).");
            }

            // -----------------------------------------------------------------
            // [ARAS API] — GET /aras/api/invoice/details
            // Query params: trxNumber (Invoice Number), customerName (Customer Name)
            // Headers: client-id, x-api-key (configured in Program.cs)
            // -----------------------------------------------------------------
            var trxNumber = searchRequest.Category == "Invoice Number" ? searchRequest.Value : null;
            var customerName = searchRequest.Category is "Invoice Customer Name" or "Customer Name"
                ? searchRequest.Value
                : null;

            var queryParams = new List<string>();
            if (!string.IsNullOrEmpty(trxNumber))
                queryParams.Add($"trxNumber={Uri.EscapeDataString(trxNumber)}");
            if (!string.IsNullOrEmpty(customerName))
                queryParams.Add($"customerName={Uri.EscapeDataString(customerName)}");

            var requestUri = "/aras/api/invoice/details"
                + (queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : string.Empty);

            var response = await _fusionHttpClient.GetAsync(requestUri);
            response.EnsureSuccessStatusCode();

            var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var envelope = await response.Content.ReadFromJsonAsync<FusionInvoiceApiEnvelope>(jsonOptions)
                ?? throw new InvalidOperationException(Exceptions.NULL_INVOICE_DETAILS);

            var fusionResult = envelope.ApiResponse
                ?? throw new InvalidOperationException(Exceptions.NULL_INVOICE_DETAILS);

            var mapped = fusionResult.Select(f => new InvoiceDetailsDto
            {
                Id = f.InvoiceNumber,
                InvoiceNumber = f.InvoiceNumber,
                InvoiceAmount = f.InvoiceAmount ?? 0,
                InvoiceBalance = f.InvoiceBalance ?? 0,
                InvoiceDate = f.InvoiceDate,
                CustomerName = f.CustomerName ?? f.CustomerNameLegacy,
                CustomerNumber = f.CustomerNumber,
                DataSource = f.DataSource ?? "AR",
                CustomerTrxId = f.CustomerTrxId,
                PaymentScheduleId = f.PaymentScheduleId
            }).ToList();

            // The generic invoice endpoint currently returns blank customer names
            // and does not apply the customer-name filter. Enrich those rows from
            // the AP invoice endpoint, which contains CUSTOMERNAME, while keeping
            // the generic endpoint's balance and amount values for this screen.
            if (searchRequest.Category is "Invoice Number" or "Invoice Customer Name" or "Invoice Customer Number")
            {
                var apRows = await GetFusionApInvoiceDetailsAsync(
                    invoiceNumber: searchRequest.Category == "Invoice Number" ? searchRequest.Value : null,
                    customerName: searchRequest.Category == "Invoice Customer Name" ? searchRequest.Value : null,
                    customerNumber: searchRequest.Category == "Invoice Customer Number" ? searchRequest.Value : null);

                var apByInvoiceNumber = apRows
                    .Where(r => !string.IsNullOrWhiteSpace(r.InvoiceNumber))
                    .GroupBy(r => r.InvoiceNumber, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

                mapped = mapped
                    .Where(r => apByInvoiceNumber.ContainsKey(r.InvoiceNumber))
                    .Select(r =>
                    {
                        var ap = apByInvoiceNumber[r.InvoiceNumber];
                        r.CustomerName = ap.CustomerName;
                        r.CustomerNumber = ap.CustomerNumber;
                        return r;
                    })
                    .Where(r => searchRequest.Category == "Invoice Number"
                        ? !string.IsNullOrWhiteSpace(r.InvoiceNumber)
                            && r.InvoiceNumber.Contains(searchRequest.Value, StringComparison.OrdinalIgnoreCase)
                        : searchRequest.Category == "Invoice Customer Name"
                            ? !string.IsNullOrWhiteSpace(r.CustomerName)
                                && r.CustomerName.Contains(searchRequest.Value, StringComparison.OrdinalIgnoreCase)
                            : !string.IsNullOrWhiteSpace(r.CustomerNumber)
                                && r.CustomerNumber.Contains(searchRequest.Value, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            return mapped
                .Where(r => min <= r.InvoiceDate && r.InvoiceDate <= max)
                .DistinctBy(r => new { r.CustomerName, r.CustomerNumber, r.InvoiceAmount, r.InvoiceDate });
        }
        // =========================================================================
        // GetAPInvoiceDetails — AP Fusion lookup for invoice-number searches.
        // Customer-name searches remain on the legacy Oracle path until Fusion
        // exposes an equivalent contract.
        // =========================================================================
        public async Task<IEnumerable<InvoiceDetailsDto>> GetAPInvoiceDetails(SearchRequestDto searchRequest)
        {
            searchRequest.Category = searchRequest.Category.Trim();
            searchRequest.Value = searchRequest.Value.Trim();

            DateTime min = searchRequest.StartDate.ToDateTime(TimeOnly.MinValue);
            DateTime max = searchRequest.EndDate.ToDateTime(TimeOnly.MaxValue);

            if (_config.IsOntest())
            {
                var list = Enumerable.Range(1, 100)
                    .Select(i => new InvoiceDetailsDto
                    {
                        Id = $"INV-{i:000}",
                        InvoiceNumber = $"7{i:00000}",
                        InvoiceAmount = 10000 + (i * 50),
                        InvoiceDate = new DateTime(2024, 1, 1).AddDays(i),
                        CustomerName = $"Customer {i}",
                        CustomerNumber = $"CUST-{1000 + i}",
                        DataSource = "AP",
                        InvoiceBalance = i % 5 == 0 ? 0 : i * 100
                    });

                if (searchRequest.Category == "Customer Name")
                    return list.Where(l => l.CustomerName == searchRequest.Value);
                if (searchRequest.Category == "Invoice Number")
                    return list.Where(l => l.InvoiceNumber == searchRequest.Value);

                throw new Exception("Invalid Search Category. Please provide correct search category (Invoice Number or Customer Name).");
            }
            if (searchRequest.Category != "Invoice Customer Name" && searchRequest.Category != "Invoice Customer Number")
                throw new Exception("Invalid Search Category. Please provide correct search category (Invoice Customer Name or Invoice Customer Number).");

            var fusionResult = await GetFusionApInvoiceDetailsAsync(
                customerName: searchRequest.Category == "Invoice Customer Name" ? searchRequest.Value : null,
                customerNumber: searchRequest.Category == "Invoice Customer Number" ? searchRequest.Value : null
            );

            return fusionResult
                .Where(r => min <= r.InvoiceDate && r.InvoiceDate <= max)
                .DistinctBy(r => new { r.CustomerName, r.CustomerNumber, r.InvoiceAmount, r.InvoiceDate });
        }
        // =========================================================================
        // GetAPInvoiceNo — ORACLE EBS (Not yet migrated to Oracle Fusion)
        // Pending: Oracle Fusion AP invoice by number endpoint from Admin.
        // Tables used: ap_invoices_all, po_vendors
        // =========================================================================
        public async Task<InvoiceAPDetailsDto> GetAPInvoiceNo(string invoiceNo)
        {
            var fusionResult = (await GetFusionApInvoiceDetailsAsync(invoiceNo)).ToList();

            return fusionResult
                .Select(r => new InvoiceAPDetailsDto
                {
                    Id = r.Id,
                    InvoiceAmount = r.InvoiceAmount,
                    InvoiceDate = r.InvoiceDate,
                    InvoiceNumber = r.InvoiceNumber,
                    CustomerName = r.CustomerName,
                    CustomerNumber = r.CustomerNumber
                })
                .FirstOrDefault()
                ?? throw new InvalidOperationException(Exceptions.NULL_INVOICE_DETAILS);
        }

        private async Task BackfillPaymentScheduleIdsAsync(IEnumerable<InvoiceDetailsDto> invoices)
        {
            var rows = invoices?
                .Where(i => i is not null && i.CustomerTrxId.HasValue && !i.PaymentScheduleId.HasValue)
                .ToList();

            if (rows is null || rows.Count == 0)
                return;

            foreach (var invoice in rows)
            {
                try
                {
                    invoice.PaymentScheduleId = await GetPaymentScheduleIdByCustomerTrxId(invoice.CustomerTrxId!.Value);
                }
                catch (Exception ex)
                {
                    // Keep invoice search usable even if reference-data lookup fails.
                    // Missing PaymentScheduleId can still be handled later in the posting path.
                    System.Diagnostics.Debug.WriteLine(
                        $"PaymentScheduleId backfill failed for CustomerTrxId {invoice.CustomerTrxId.Value}: {ex.Message}");
                }
            }
        }

        private async Task<long?> GetPaymentScheduleIdByCustomerTrxId(long customerTrxId)
        {
            var url = $"/aras/api/adjustment-reference-data?customerTrxId={customerTrxId}";
            var response = await _fusionHttpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return null;

            var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var envelope = await response.Content.ReadFromJsonAsync<AdjustmentReferenceDataResponse>(jsonOptions);
            return envelope?.Data?.FirstOrDefault()?.PaymentScheduleId;
        }
        // =========================================================================
        // GetSRAutoNetCNDetails — ORACLE EBS (Not yet migrated to Oracle Fusion)
        // Pending: Oracle Fusion Credit Note endpoint from Admin.
        // Tables used: ra_customer_trx_all, ra_customer_trx_lines_all, zx_lines
        // =========================================================================
        public async Task<IEnumerable<SearchCNDetailsRowDto>> GetSRAutoNetCNDetails(string invoiceNumber)
        {
            // Feature parked for now by Finance.
            // Returning an empty result prevents Oracle DB access and the old EBS path from firing.
            return [];
        }

        public async Task<IEnumerable<InvoiceDetailsDto>> GetCnInvoiceDetails(string invoiceNo)
        {
            // Feature parked for now by Finance.
            // Returning an empty list avoids any Oracle DB access and prevents the old
            // Oracle/EBS path from throwing connection errors while the feature is disabled.
            return [];
        }
        // =========================================================================
        // GetOneInvoiceDetails — ORACLE EBS (Not yet migrated to Oracle Fusion)
        // Pending: Oracle Fusion single-invoice lookup endpoint from Admin.
        // Tables used: ra_customer_trx_all, ra_cust_trx_types_all,
        //              hz_cust_accounts, ar_payment_schedules_all
        // =========================================================================
        public async Task<InvoiceDetailsDto> GetOneInvoiceDetails(InvoiceDetailsRequestDto invoice)
        {
            if (_config.IsOntest())
            {
                return Enumerable.Range(1, 5)
                    .Select(i => new InvoiceDetailsDto
                    {
                        Id = $"INV-{i:000}",
                        InvoiceNumber = $"5{i:00000}",
                        InvoiceAmount = 10000 + (i * 50),
                        InvoiceDate = new DateTime(2024, 1, 1).AddDays(i),
                        CustomerName = $"Customer {i}",
                        CustomerNumber = $"CUST-{1000 + i}",
                        DataSource = "AR",
                        InvoiceBalance = i % 5 == 0 ? 0 : i * 100
                    }).FirstOrDefault() ?? throw new Exception("Invalid Search Category. Please provide correct search category (Invoice Number or Customer Name).");
            }
            await using var conn = await oracleConnection.OpenWithPolicyContextAsync();

            // [ORACLE EBS] — Single AR Invoice lookup
            // TODO: Replace with Oracle Fusion endpoint when provided by Admin.
            var sql = @"
				 SELECT   apsa.amount_due_original InvoiceAmount,
						   apsa.amount_due_remaining InvoiceBalance,
						   rct.trx_date InvoiceDate,
						   rct.trx_number InvoiceNumber,
						   hca.account_name CustomerName,
						   hca.account_number CustomerNumber,
						   'AR' DataSource
					FROM   ra_customer_trx_all rct,
						   ra_cust_trx_types_all ctt,
						   hz_cust_accounts hca,
						   ar_payment_schedules_all apsa
					WHERE       rct.cust_trx_type_id = ctt.cust_trx_type_id
						   AND rct.bill_to_customer_id = hca.cust_account_id
						   AND rct.customer_trx_id = apsa.customer_trx_id
						   AND TRIM(rct.trx_number) = UPPER(:invnumb)
						   AND apsa.amount_due_original = :invamnt
						   AND rct.trx_date = :invdate
						   AND TRIM(hca.account_name) = UPPER(:custname)
						   AND hca.account_number = :custnumb
					ORDER BY   rct.trx_date DESC, rct.trx_number
				";

            dynamic param = new
            {
                invnumb = invoice.InvoiceNumber,
                invamnt = invoice.InvoiceAmount,
                invdate = invoice.InvoiceDate.ToDateTime(TimeOnly.MinValue),
                custname = invoice.CustomerName,
                custnumb = invoice.CustomerNumber,
            };

            var result = await conn.QueryAsync<InvoiceDetailsDto>(
                sql,
                (object)param,
                commandTimeout: 120
            ) ?? throw new InvalidOperationException(Exceptions.NULL_INVOICE_DETAILS);

            // ---------------------------------------------------------------
            // FIXED: Use FirstOrDefault with a null-check throw instead of
            // silently returning null, so the controller catches it properly
            // and wraps it in a Failed ResponseDto with a meaningful message.
            // ---------------------------------------------------------------
            return result
                .DistinctBy(r => new { r.CustomerName, r.CustomerNumber, r.InvoiceAmount, r.InvoiceDate })
                .FirstOrDefault()
                ?? throw new InvalidOperationException(Exceptions.NULL_INVOICE_DETAILS);
        }

        // =========================================================================
        // GetCustomerTrxIdByInvoiceDetails — ORACLE EBS (Not yet migrated to Oracle Fusion)
        // Pending: Oracle Fusion transaction ID lookup endpoint from Admin.
        // Tables used: ra_customer_trx_all, ra_cust_trx_types_all,
        //              hz_cust_accounts, ar_payment_schedules_all
        // =========================================================================
        public async Task<string> GetCustomerTrxIdByInvoiceDetails(CustomerInvoiceRequestDto invoiceDetails)
        {
            var conn = await oracleConnection.OpenWithoutPolicyAsync();

            // [ORACLE EBS] — Customer transaction ID lookup
            // TODO: Replace with Oracle Fusion endpoint when provided by Admin.
            var sql = @"
				SELECT
					rct.customer_trx_id
				FROM
					ra_customer_trx_all rct,
					ra_cust_trx_types_all ctt,
					hz_cust_accounts hca,
					ar_payment_schedules_all apsa
				WHERE
					rct.cust_trx_type_id = ctt.cust_trx_type_id
					AND rct.bill_to_customer_id = hca.cust_account_id
					AND rct.customer_trx_id = apsa.customer_trx_id
    
					AND TRIM(rct.trx_number) = UPPER(:invnumb)
					AND rct.trx_date = :invdate
					AND TRIM(hca.account_name) = UPPER(:custname)
					AND hca.account_number = :custnumb";

            dynamic param = new
            {
                invnumb = invoiceDetails.InvoiceNumber,
                invdate = invoiceDetails.InvoiceDate.ToDateTime(TimeOnly.MinValue),
                custname = invoiceDetails.CustomerName,
                custnumb = invoiceDetails.CustomerNumber,
            };

            var result = await conn.QueryFirstAsync<string>(
                sql,
                (object)param,
                commandTimeout: 120
            ) ?? throw new InvalidOperationException(Exceptions.NULL_INVOICE_DETAILS);

            return result;
        }

        public async Task<string> GetCustomerTrxIdByInvoiceDetails(OracleConnection oracleConnection, CustomerInvoiceRequestDto invoiceDetails)
        {
            // [ORACLE EBS] — Customer transaction ID lookup (overload with explicit connection)
            // TODO: Replace with Oracle Fusion endpoint when provided by Admin.
            var sql = @"
				SELECT
					rct.customer_trx_id
				FROM
					ra_customer_trx_all rct,
					ra_cust_trx_types_all ctt,
					hz_cust_accounts hca,
					ar_payment_schedules_all apsa
				WHERE
					rct.cust_trx_type_id = ctt.cust_trx_type_id
					AND rct.bill_to_customer_id = hca.cust_account_id
					AND rct.customer_trx_id = apsa.customer_trx_id
    
					AND TRIM(rct.trx_number) = UPPER(:invnumb)
					AND rct.trx_date = :invdate
					AND TRIM(hca.account_name) = UPPER(:custname)
					AND hca.account_number = :custnumb";

            dynamic param = new
            {
                invnumb = invoiceDetails.InvoiceNumber,
                invdate = invoiceDetails.InvoiceDate.ToDateTime(TimeOnly.MinValue),
                custname = invoiceDetails.CustomerName,
                custnumb = invoiceDetails.CustomerNumber,
            };

            var result = await oracleConnection.QueryFirstOrDefaultAsync<string>(
                sql,
                (object)param,
                commandTimeout: 120
            ) ?? throw new InvalidOperationException(Exceptions.INVALID_CUSTOMER_TRX_ID);

            return result;
        }
    }
}
