using ARAS.Main.Oracle.Api.App_Code.Globals.Constants;
using ARAS.Main.Oracle.Api.Context;
using ARAS.Main.Oracle.Api.Factories.Interfaces;
using ARAS.Main.Oracle.Api.Models.Dtos;
using ARAS.Main.Oracle.Api.Repositories.Interfaces;
using ARAS.Main.Oracle.Api.Services.Interfaces;
using Dapper;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.DirectoryServices.Protocols;
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

        // =========================================================================
        // GetInvoiceDetails — MIGRATED TO ORACLE FUSION (Integration Hub REST API)
        // Previously: Direct SQL query against Oracle EBS AR schema (ra_customer_trx_all, etc.)
        // Now: HTTP GET /fin/api/masterdata/financials/invoice-details
        // =========================================================================
        public async Task<IEnumerable<InvoiceDetailsDto>> GetInvoiceDetails(SearchRequestDto searchRequest)
        {
            searchRequest.Category = searchRequest.Category.Trim();
            searchRequest.Value = searchRequest.Value.Trim();

            DateTime min = searchRequest.StartDate.ToDateTime(TimeOnly.MinValue);
            DateTime max = searchRequest.EndDate.ToDateTime(TimeOnly.MaxValue);

            // -----------------------------------------------------------------
            // Test/mock data path — unchanged from EBS version
            // -----------------------------------------------------------------
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
            // [ORACLE EBS — RETIRED] Direct DB query via ODP.NET + Dapper
            // Kept for documentation and rollback reference.
            // Tables used: ra_customer_trx_all, ra_cust_trx_types_all,
            //              hz_cust_accounts, ar_payment_schedules_all
            // -----------------------------------------------------------------
            //
            // await using var conn = await oracleConnection.OpenWithPolicyContextAsync();
            //
            // var sql = @"
            //       SELECT   apsa.amount_due_original InvoiceAmount,
            //                apsa.amount_due_remaining InvoiceBalance,
            //                rct.trx_date InvoiceDate,
            //                rct.trx_number InvoiceNumber,
            //                hca.account_name CustomerName,
            //                hca.account_number CustomerNumber,
            //                'AR' DataSource,
            //                CASE
            //                   WHEN apsa.amount_due_remaining = 0 THEN 'CLOSED'
            //                   ELSE 'OPEN'
            //                END AS InvoiceStatus
            //         FROM   ra_customer_trx_all rct,
            //                ra_cust_trx_types_all ctt,
            //                hz_cust_accounts hca,
            //                ar_payment_schedules_all apsa
            //        WHERE       rct.cust_trx_type_id = ctt.cust_trx_type_id
            //                AND rct.bill_to_customer_id = hca.cust_account_id
            //                AND rct.customer_trx_id = apsa.customer_trx_id
            //                AND TRIM (hca.account_name) = NVL (UPPER (:custname), hca.account_name)
            //                AND TRIM (rct.trx_number) = NVL (UPPER (:trxno), rct.trx_number)
            //     ORDER BY   rct.trx_date DESC, rct.trx_number";
            //
            // dynamic param = new
            // {
            //     trxno = searchRequest.Category == "Invoice Number" ? searchRequest.Value : null,
            //     custname = searchRequest.Category == "Customer Name" ? searchRequest.Value : null
            // };
            //
            // var result = await conn.QueryAsync<InvoiceDetailsDto>(
            //     sql,
            //     (object)param,
            //     commandTimeout: 120
            // ) ?? throw new InvalidOperationException(Exceptions.NULL_INVOICE_DETAILS);
            //
            // return result
            //     .Where(r => min <= r.InvoiceDate && r.InvoiceDate <= max)
            //     .DistinctBy(r => new { r.CustomerName, r.CustomerNumber, r.InvoiceAmount, r.InvoiceDate });
            // -----------------------------------------------------------------

            // -----------------------------------------------------------------
            // [ORACLE FUSION] — Integration Hub REST API
            // Endpoint: GET /fin/api/masterdata/financials/invoice-details
            // Headers:  client-id, x-api-key (configured in Program.cs via IHttpClientFactory)
            // -----------------------------------------------------------------
            var customerName = searchRequest.Category == "Customer Name" ? searchRequest.Value : null;
            var invoiceNumber = searchRequest.Category == "Invoice Number" ? searchRequest.Value : null;

            var queryParams = new List<string>();
            if (!string.IsNullOrEmpty(customerName))
                queryParams.Add($"customerName={Uri.EscapeDataString(customerName)}");
            if (!string.IsNullOrEmpty(invoiceNumber))
                queryParams.Add($"invoiceNumber={Uri.EscapeDataString(invoiceNumber)}");

            var requestUri = "/fin/api/masterdata/financials/invoice-details"
                + (queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : string.Empty);

            var response = await _fusionHttpClient.GetAsync(requestUri);
            response.EnsureSuccessStatusCode();

            // ---------------------------------------------------------------
            // Oracle Fusion API envelope:
            // { "statusCode": 200, "message": "Success", "apiResponse": [...] }
            // "invoiceAmmount" is a known typo in the API — handled via
            // [JsonPropertyName] on FusionInvoiceDetailsResponse.
            // "invoiceAmmount" and "invoiceBalance" are nullable in the response.
            // ---------------------------------------------------------------
            var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var envelope = await response.Content.ReadFromJsonAsync<FusionInvoiceApiEnvelope>(jsonOptions)
                ?? throw new InvalidOperationException(Exceptions.NULL_INVOICE_DETAILS);

            var fusionResult = envelope.ApiResponse
                ?? throw new InvalidOperationException(Exceptions.NULL_INVOICE_DETAILS);

            // Map Fusion response to the shared InvoiceDetailsDto
            // Null amounts default to 0 to keep compatibility with existing UI logic
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
        // GetAPInvoiceDetails — ORACLE EBS (Not yet migrated to Oracle Fusion)
        // Pending: Oracle Fusion AP invoice endpoint from Admin.
        // Tables used: ap_invoices_all, po_vendors
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
            await using var conn = await oracleConnection.OpenWithPolicyContextAsync();

            // [ORACLE EBS] — AP Invoices query
            // TODO: Replace with Oracle Fusion AP invoice endpoint when provided by Admin.
            var sql = @"
				SELECT
					apa.invoice_num InvoiceNumber,
					apa.amount_paid InvoiceAmount,
					apa.invoice_date InvoiceDate,
					pv.vendor_name CustomerName,
					pv.vendor_id CustomerNumber,
					'AP' DataSource
				FROM
					ap_invoices_all apa, 
					po_vendors pv
				WHERE
					apa.vendor_id = pv.vendor_id 
					AND TRIM(invoice_num) = NVL(UPPER(:trxno), invoice_num)
					AND TRIM(pv.vendor_name) = NVL(UPPER(:custname), pv.vendor_name)
				ORDER BY
					apa.invoice_date DESC, 
					invoice_num
				";

            dynamic param = new
            {
                trxno = searchRequest.Category == "Invoice Number" ? searchRequest.Value : null,
                custname = searchRequest.Category == "Customer Name" ? searchRequest.Value : null
            };

            var result = await conn.QueryAsync<InvoiceDetailsDto>(
                sql,
                (object)param,
                commandTimeout: 120
            ) ?? throw new InvalidOperationException(Exceptions.NULL_INVOICE_DETAILS);

            return result.Where(r => min <= r.InvoiceDate && r.InvoiceDate <= max).DistinctBy(r => new { r.CustomerName, r.CustomerNumber, r.InvoiceAmount, r.InvoiceDate });
        }

        // =========================================================================
        // GetAPInvoiceNo — ORACLE EBS (Not yet migrated to Oracle Fusion)
        // Pending: Oracle Fusion AP invoice by number endpoint from Admin.
        // Tables used: ap_invoices_all, po_vendors
        // =========================================================================
        public async Task<InvoiceAPDetailsDto> GetAPInvoiceNo(string invoiceNo)
        {
            await using var conn = await oracleConnection.OpenWithoutPolicyAsync();
            invoiceNo = invoiceNo.Trim();

            // [ORACLE EBS] — AP Invoice by number query
            // TODO: Replace with Oracle Fusion endpoint when provided by Admin.
            var sql = @"
					SELECT    apa.invoice_num AS InvoiceNumber,
							  apa.amount_paid AS InvoiceAmount,
							  apa.invoice_date AS InvoiceDate,
							  pv.vendor_name AS CustomerName,
							  pv.vendor_id AS CustomerNumber
					FROM      ap_invoices_all apa,
							  po_vendors pv
					WHERE     apa.vendor_id = pv.vendor_id AND TRIM(apa.invoice_num) = UPPER(:trxno)
					ORDER BY  apa.invoice_date DESC, apa.invoice_num
				";

            return await conn.QueryFirstOrDefaultAsync<InvoiceAPDetailsDto>(
                sql,
                new { trxno = $"{invoiceNo}" },
                commandTimeout: 120
            ) ?? throw new InvalidOperationException(Exceptions.NULL_INVOICE_DETAILS);
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

            return result.DistinctBy(r => new { r.CustomerName, r.CustomerNumber, r.InvoiceAmount, r.InvoiceDate }).FirstOrDefault();
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
