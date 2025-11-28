using Dapper;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;
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

		public InvoiceRepository(MainDbContext efContext, IOracleConnectionFactory oracleConnection)
		{
			this.efContext = efContext;
			this.oracleConnection = oracleConnection;
		}

		public async Task<IEnumerable<InvoiceDetailsDto>> GetInvoiceDetails(string invoiceNumber)
		{
			await using var conn = await oracleConnection.OpenWithPolicyContextAsync();
			invoiceNumber = invoiceNumber.Trim();

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
	}
}
