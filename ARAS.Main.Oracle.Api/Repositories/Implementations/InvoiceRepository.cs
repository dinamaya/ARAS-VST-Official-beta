using Dapper;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using ARAS.Main.Oracle.Api.Context;
using ARAS.Main.Oracle.Api.Repositories.Interfaces;
using ARAS.Main.Oracle.Api.Models.Dtos;

namespace ARAS.Main.Oracle.Api.Repositories.Implementations
{
	public class InvoiceRepository : IInvoiceRepository
	{
		private readonly MainDbContext efContext;
		private readonly Func<Task<OracleConnection>> dpContext;

		public InvoiceRepository(MainDbContext efContext, Func<Task<OracleConnection>> dpContext)
		{
			this.efContext = efContext;
			this.dpContext = dpContext;
		}

		public async Task<InvoiceDetailsDto> GetInvoiceNo(string invoiceNo)
		{
			await using var conn = await dpContext();

			var sql = @"
					SELECT DISTINCT rctv.trx_number AS InvoiceNumber
					FROM RA_CUSTOMER_TRX_PARTIAL_V rctv
					JOIN ra_customer_trx_lines_all rctla
					  ON rctla.customer_trx_id = rctv.customer_trx_id
					WHERE UPPER(TRIM(rctv.trx_number)) = UPPER(:trxno)
				";

			var result = await conn.QueryAsync<InvoiceDetailsDto>(
				sql,
				new { trxno = $"{invoiceNo}"},
				commandTimeout: 120
			);

			return result.Select(i => new InvoiceDetailsDto()
			{
				Id = Guid.NewGuid().ToString(),
				InvoiceAmount = 2_000_329.00d,
				InvoiceNumber = i.InvoiceNumber,
				InvoiceDate = DateTime.Now.AddDays(34),
				CustomerName = "Customer First M. Last",
				CustomerNumber = "09123456789",
				OtherDetails = "Other Details here"
			}).FirstOrDefault();
		}

	}
}
