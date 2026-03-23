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
using System.Numerics;

namespace ARAS.Main.Oracle.Api.Repositories.Implementations
{
	public class AdjustmentRepository : IAdjustmentRepository
	{
		private readonly MainDbContext efContext;
		private readonly IOracleConnectionFactory oracleConnection;
		private readonly IInvoiceRepository invoiceRepo;
		private readonly IConfigurationService _config;
		private readonly ILogger<IAdjustmentRepository> _logger;

        public AdjustmentRepository(MainDbContext efContext, IOracleConnectionFactory oracleConnection, IConfigurationService config, ILogger<IAdjustmentRepository> logger, IInvoiceRepository invoiceRepo)
        {
            this.efContext = efContext;
            this.oracleConnection = oracleConnection;
            _config = config;
            _logger = logger;
            this.invoiceRepo = invoiceRepo;
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
			var conn = await oracleConnection.OpenWithoutPolicyAsync();
			var list = new List<AdjustmentCreateStagingRow>();

			foreach(var item in data)
			{
				var receivableActivity = await GetReceivableActivityByName(conn, item.AdjustmentActivity);
				string customerTrxId = await invoiceRepo.GetCustomerTrxIdByInvoiceDetails(conn, new()
				{
					InvoiceNumber = item.InvoiceNumber,
					InvoiceDate = DateOnly.FromDateTime(item.InvoiceDate),
					CustomerName = item.CustomerName,
					CustomerNumber = item.CustomerNumber,
				});
				
				list.Add(new AdjustmentCreateStagingRow {
					ReceivableActivityId = receivableActivity.Id,
					CustomerTrxId = customerTrxId,
					AdjustmentsDetails = item
				});
			}

			await CreateStageRow(list);
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
			if (!adjustmentIds.Any()) return [];

			var conn = await oracleConnection.OpenWithoutPolicyAsync();

			string sql = @"
				SELECT 
					stg.HEADER_ID HeaderId,
					rta.NAME AdjustmentActivityName
				FROM 
					APPS.XXMSI_AR_ADJ_STG stg, 
					(
						SELECT   DISTINCT
							RECEIVABLES_TRX_ID Id,
							NAME Name
						FROM   ar_receivables_trx_all
						WHERE   TYPE = 'ADJUST'
					) rta
				WHERE   
					stg.RECEIVABLES_TRX_ID = rta.Id AND 
					stg.STG_FLAG = '1' AND 
					stg.INT_FLAG = '1' AND 
					stg.INV_FLAG = '1'";

			var result = await conn.QueryAsync<PostedResponseDto>(sql,commandTimeout: 120) ?? throw new Exception(Exceptions.INVALID_RECEIVABLE_ACTIVITY);
			return result.Join(adjustmentIds, r => r.HeaderId, id => id, (r, id) => r);
		}
    }
}
