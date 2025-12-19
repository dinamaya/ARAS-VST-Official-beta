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
using System.Numerics;

namespace ARAS.Main.Oracle.Api.Repositories.Implementations
{
	public class AdjustmentRepository : IAdjustmentRepository
	{
		private readonly MainDbContext efContext;
		private IOracleConnectionFactory oracleConnection;
		private readonly IConfigurationService _config;

		public AdjustmentRepository(MainDbContext efContext, IOracleConnectionFactory oracleConnection, IConfigurationService config)
		{
			this.efContext = efContext;
			this.oracleConnection = oracleConnection;
			_config = config;
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

		public async Task Create(AdjustmentPostingDto data)
		{
			await using var conn = await oracleConnection.OpenWithoutPolicyAsync();

			var sql = @"
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
					VALUES(
						:AdjustmentId,
						:CustomerTRXId,
						:InvoiceNumber,
						:AdjustmentAmount,
						'ADJUSTMENT API',
						:GLDate,
						'LINE',
						:PaymentScheduleId, -- IS IT NULL?
						:DateApplied,
						:TransactionTypeId,
						:ReasonCode,
						:Remarks,
						:AccountName,
						:AccountNumber,
						1,
						0,
						0
					)
				";

			var param = new {
				AdjustmentId = data.AdjustmentId,
				CustomerTRXId = data.CustomerTRXId,
				InvoiceNumber = data.InvoiceNumber,
				AdjustmentAmount = data.AdjustmentAmount,
				GLDate = data.GLDate,
				PaymentScheduleId = data.PaymentScheduleId,
				DateApplied = data.DateApplied,
				TransactionTypeId = data.TransactionTypeId,
				ReasonCode = data.ReasonCode,
				Remarks = data.Remarks,
				AccountName = data.AccountName,
				AccountNumber = data.AccountNumber
			};

			await conn.ExecuteAsync(
				sql,
				param,
				commandTimeout: 120
			);
		}
	}
}
