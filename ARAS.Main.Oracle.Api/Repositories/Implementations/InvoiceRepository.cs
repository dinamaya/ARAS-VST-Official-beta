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

        private async Task<IEnumerable<InvoiceDetailsDto>> GetFusionApInvoiceDetailsAsync(string? invoiceNumber = null, string? customerName = null)
        {
            var queryParams = new List<string>();

            if (!string.IsNullOrWhiteSpace(invoiceNumber))
                queryParams.Add($"trxNumber={Uri.EscapeDataString(invoiceNumber.Trim())}");

            if (!string.IsNullOrWhiteSpace(customerName))
                queryParams.Add($"customerName={Uri.EscapeDataString(customerName.Trim())}");

            var requestUri = "/aras/api/ap/invoice/details"
                + (queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : string.Empty);

            var response = await _fusionHttpClient.GetAsync(requestUri);
            response.EnsureSuccessStatusCode();

            var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var envelope = await response.Content.ReadFromJsonAsync<FusionInvoiceApiEnvelope>(jsonOptions)
                ?? throw new InvalidOperationException(Exceptions.NULL_INVOICE_DETAILS);

            var fusionResult = envelope.ApiResponse
                ?? throw new InvalidOperationException(Exceptions.NULL_INVOICE_DETAILS);

            return fusionResult.Select(f => new InvoiceDetailsDto
            {
                Id = f.InvoiceNumber,
                InvoiceNumber = f.InvoiceNumber,
                InvoiceAmount = f.InvoiceAmount ?? 0,
                InvoiceDate = f.InvoiceDate,
                CustomerName = f.CustomerName,
                CustomerNumber = f.CustomerNumber,
                DataSource = f.DataSource ?? "AP",
                InvoiceBalance = f.InvoiceBalance ?? 0
            });
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

                if (searchRequest.Category == "Customer Name")
                    return list.Where(l => l.CustomerName == searchRequest.Value);
                if (searchRequest.Category == "Invoice Number")
                    return list.Where(l => l.InvoiceNumber == searchRequest.Value);

                throw new Exception("Invalid Search Category. Please provide correct search category (Invoice Number or Customer Name).");
            }

            // -----------------------------------------------------------------
            // [ARAS API] — GET /aras/api/invoice/details
            // Query params: trxNumber (Invoice Number), customerName (Customer Name)
            // Headers: client-id, x-api-key (configured in Program.cs)
            // -----------------------------------------------------------------
            var trxNumber   = searchRequest.Category == "Invoice Number" ? searchRequest.Value : null;
            var customerName = searchRequest.Category == "Customer Name"  ? searchRequest.Value : null;

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
                CustomerName = f.CustomerName,
                CustomerNumber = f.CustomerNumber,
                DataSource = f.DataSource ?? "AR"
            });

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
            if (searchRequest.Category != "Invoice Number" && searchRequest.Category != "Customer Name")
                throw new Exception("Invalid Search Category. Please provide correct search category (Invoice Number or Customer Name).");

            var fusionResult = await GetFusionApInvoiceDetailsAsync(
                searchRequest.Category == "Invoice Number" ? searchRequest.Value : null,
                searchRequest.Category == "Customer Name" ? searchRequest.Value : null
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

        // =========================================================================
        // GetSRAutoNetCNDetails — ORACLE EBS (Not yet migrated to Oracle Fusion)
        // Pending: Oracle Fusion Credit Note endpoint from Admin.
        // Tables used: ra_customer_trx_all, ra_customer_trx_lines_all, zx_lines
        // =========================================================================
        public async Task<IEnumerable<SearchCNDetailsRowDto>> GetSRAutoNetCNDetails(string invoiceNumber)
        {
            invoiceNumber = invoiceNumber.Trim();

            if (_config.IsOntest())
                return Enumerable.Range(1, 100)
                    .Select(i => new SearchCNDetailsRowDto
                    {
                        Id = $"CN-{i:000}",
                        CNRef = $"7{i:00000}",
                        CNAmt = 10000 + (i * 50),
                        WT = 500 + (i * 5),
                        InvoiceDate = new DateTime(2024, 1, 1).AddDays(i),
                        CustomerName = $"Customer {i}",
                        CustomerNumber = $"CUST-{1000 + i}"
                    }).Where(c => c.CNRef.Equals(invoiceNumber));

            await using var conn = await oracleConnection.OpenWithPolicyContextAsync();

            // [ORACLE EBS] — Credit Note details via AR + Tax schema
            // TODO: Replace with Oracle Fusion Credit Note endpoint when provided by Admin.
            var sql = @"
					SELECT   
						rcta.trx_number cn_ref,
						SUM (rctla.gross_extended_amount) cn_amount,
						SUM (zl.tax_amt) wt
					FROM
						ra_customer_trx_all rcta,
						ra_customer_trx_lines_all rctla,
						zx_lines zl
					WHERE
						rcta.customer_trx_id = rctla.customer_trx_id
						AND rctla.customer_trx_line_id = zl.trx_line_id(+)
						AND rctla.line_type = 'LINE'
						AND zl.tax_jurisdiction_code(+) = 'MSI_PH_CWTAX'
						AND TRIM(rcta.trx_number) = UPPER(:trxno)
					GROUP BY   rcta.trx_number
					ORDER BY   rcta.trx_number
				";

            var result = await conn.QueryAsync<SearchCNDetailsRowDto>(
                sql,
                new { trxno = $"{invoiceNumber}" },
                commandTimeout: 120
            ) ?? throw new InvalidOperationException(Exceptions.NULL_INVOICE_DETAILS);

            return result.DistinctBy(r => new { r.CNRef });
        }

        // =========================================================================
        // GetCnInvoiceDetails — ORACLE EBS (Not yet migrated to Oracle Fusion)
        // Pending: Oracle Fusion CN invoice endpoint from Admin.
        // Tables used: ra_customer_trx_all, ra_customer_trx_lines_all,
        //              hz_cust_accounts, ra_cust_trx_types_all
        // =========================================================================
        public async Task<IEnumerable<InvoiceDetailsDto>> GetCnInvoiceDetails(string invoiceNo)
        {
            invoiceNo = invoiceNo.Trim();

            if (_config.IsOntest())
                return Enumerable.Range(1, 100)
                    .Select(i => new InvoiceDetailsDto
                    {
                        Id = $"INV-{i:000}",
                        InvoiceNumber = $"5{i:00000}",
                        InvoiceAmount = 10000 + (i * 50),
                        InvoiceDate = new DateTime(2024, 1, 1).AddDays(i),
                        CustomerName = $"Customer {i}",
                        CustomerNumber = $"CUST-{1000 + i}"
                    }).Where(c => c.InvoiceNumber.Equals(invoiceNo));

            await using var conn = await oracleConnection.OpenWithPolicyContextAsync();

            // [ORACLE EBS] — CN Invoice details via AR schema
            // TODO: Replace with Oracle Fusion endpoint when provided by Admin.
            var sql = @"
					    SELECT   
							rcta.trx_number InvoiceNumber,
							SUM (rctla.gross_extended_amount) InvoiceAmount,
							rcta.trx_date InvoiceDate,
							hca.account_name CustomerName,
							hca.account_number CustomerNumber,
							rctt.name cust_trx_type
						FROM
							ra_customer_trx_all rcta,
							ra_customer_trx_lines_all rctla,
							hz_cust_accounts hca,
							ra_cust_trx_types_all rctt
						WHERE
							rcta.customer_trx_id = rctla.customer_trx_id
							AND rcta.bill_to_customer_id = hca.cust_account_id
							AND rctla.line_type = 'LINE'
							AND rcta.cust_trx_type_id = rctt.cust_trx_type_id
							AND rcta.cust_trx_type_id = rctt.cust_trx_type_id
							AND rctt.org_id = 101
							AND TRIM(rcta.trx_number) = UPPER(:trxno)
						GROUP BY
							rcta.trx_number,
							rcta.trx_date,
							hca.account_name,
							hca.account_number,
							rctt.name
						ORDER BY
							rcta.trx_number
				";

            var result = await conn.QueryAsync<InvoiceDetailsDto>(
                sql,
                new { trxno = $"{invoiceNo}" },
                commandTimeout: 120
            ) ?? throw new InvalidOperationException(Exceptions.NULL_INVOICE_DETAILS);

            return result;
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
