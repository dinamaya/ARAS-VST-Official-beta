using Dapper;
using System.Data;
using ARAS.Main.Oracle.Api.Context;
using ARAS.Main.Oracle.Api.Repositories.Interfaces;
using ARAS.Main.Oracle.Api.Models.Dtos;
using ARAS.Main.Oracle.Api.App_Code.Globals.Constants;
using ARAS.Main.Oracle.Api.Factories.Interfaces;
using ARAS.Main.Oracle.Api.Services.Interfaces;

namespace ARAS.Main.Oracle.Api.Repositories.Implementations
{
	public class InvoiceRepository : IInvoiceRepository
	{
		private readonly MainDbContext efContext;
		private readonly IOracleConnectionFactory oracleConnection;
		private readonly IConfigurationService _config;

		public InvoiceRepository(MainDbContext efContext, IOracleConnectionFactory oracleConnection, IConfigurationService config)
		{
			this.efContext = efContext;
			this.oracleConnection = oracleConnection;
			_config = config;
		}

		public async Task<IEnumerable<InvoiceDetailsDto>> GetInvoiceDetails(string invoiceNumber)
		{
			invoiceNumber = invoiceNumber.Trim();

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
					}).Where(c => c.InvoiceNumber.Equals(invoiceNumber));

			await using var conn = await oracleConnection.OpenWithPolicyContextAsync();

			var sql = @"
					    SELECT   apsa.amount_due_original InvoiceAmount,
							   rct.trx_date InvoiceDate,
							   rct.trx_number InvoiceNumber,
							   hca.account_name CustomerName,
							   hca.account_number CustomerNumber
						FROM   ra_customer_trx_all rct,
							   ra_cust_trx_types_all ctt,
							   hz_cust_accounts hca,
							   ar_payment_schedules_all apsa
					   WHERE       rct.cust_trx_type_id = ctt.cust_trx_type_id
							   AND rct.bill_to_customer_id = hca.cust_account_id
							   AND rct.customer_trx_id = apsa.customer_trx_id
							   AND TRIM(rct.trx_number) = UPPER(:trxno)
					ORDER BY   rct.trx_date DESC, rct.trx_number
				";
			var result = await conn.QueryAsync<InvoiceDetailsDto>(
				sql,
				new { trxno = $"{invoiceNumber}" },
				commandTimeout: 120
			) ?? throw new InvalidOperationException(Exceptions.NULL_INVOICE_DETAILS);

			return result.DistinctBy(r => new { r.CustomerName, r.CustomerNumber, r.InvoiceAmount, r.InvoiceDate });
		}

		public async Task<InvoiceDetailsDto> GetInvoiceNo(string invoiceNo)
		{
			await using var conn = await oracleConnection.OpenWithPolicyContextAsync();
			invoiceNo = invoiceNo.Trim();

			var sql = @"
					    SELECT   apsa.amount_due_original InvoiceAmount,
							   rct.trx_date InvoiceDate,
							   rct.trx_number InvoiceNumber,
							   hca.account_name CustomerName,
							   hca.account_number CustomerNumber
						FROM   ra_customer_trx_all rct,
							   ra_cust_trx_types_all ctt,
							   hz_cust_accounts hca,
							   ar_payment_schedules_all apsa
					   WHERE       rct.cust_trx_type_id = ctt.cust_trx_type_id
							   AND rct.bill_to_customer_id = hca.cust_account_id
							   AND rct.customer_trx_id = apsa.customer_trx_id
							   AND TRIM(rct.trx_number) = UPPER(:trxno)
					ORDER BY   rct.trx_date DESC, rct.trx_number
				";

			return await conn.QueryFirstOrDefaultAsync<InvoiceDetailsDto>(
				sql,
				new { trxno = $"{invoiceNo}"},
				commandTimeout: 120
			) ?? throw new InvalidOperationException(Exceptions.NULL_INVOICE_DETAILS);
		}

        public async Task<InvoiceAPDetailsDto> GetAPInvoiceNo(string invoiceNo)
        {
            await using var conn = await oracleConnection.OpenWithoutPolicyAsync();
            invoiceNo = invoiceNo.Trim();

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
	}
}
