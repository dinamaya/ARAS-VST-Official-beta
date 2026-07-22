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

        public AdjustmentRepository(MainDbContext efContext, IOracleConnectionFactory oracleConnection, IConfigurationService config, ILogger<IAdjustmentRepository> logger, IInvoiceRepository invoiceRepo, IHttpClientFactory httpClientFactory)
        {
            this.efContext = efContext;
            this.oracleConnection = oracleConnection;
            _config = config;
            _logger = logger;
            this.invoiceRepo = invoiceRepo;
			_httpClient = httpClientFactory.CreateClient();
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
				.GroupBy(item => item.HeaderId)
				.Select(group => group.First())
				.ToList();

			foreach(var item in rows)
			{
				if (item.CustomerTrxId is null)
					throw new InvalidOperationException($"CustomerTrxId is required for Fusion adjustment posting. Invoice: {item.InvoiceNumber}");

				await CreateFusionAdjustment(item);
			}
		}

		private async Task CreateFusionAdjustment(AdjustmentPostingDto item)
		{
			using var request = new HttpRequestMessage(HttpMethod.Post, _config.GetFusionAdjustmentEndpoint());
			request.Headers.Authorization = CreateBasicAuthenticationHeader();
			request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/xml"));
			request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("identity"));
			request.Headers.TryAddWithoutValidation("SOAPAction", $"\"{_config.GetFusionAdjustmentSoapAction()}\"");
			request.Content = new StringContent(CreateFusionAdjustmentEnvelope(item), Encoding.UTF8, "text/xml");

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

		private string CreateFusionAdjustmentEnvelope(AdjustmentPostingDto item)
		{
			var applyDate = item.DateApplied.ToString("yyyy-MM-dd");
			var glDate = item.DateApplied.ToString("yyyy-MM-dd");
			var amount = Convert.ToDecimal(item.AdjustmentAmount).ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
			var receivablesTrxId = _config.GetFusionAdjustmentReceivablesTrxId(item.AdjustmentActivity);
			var paymentScheduleId = item.PaymentScheduleId ?? _config.GetFusionAdjustmentDefaultPaymentScheduleId();

			return $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/""
                  xmlns:typ=""http://xmlns.oracle.com/apps/financials/receivables/transactions/adjustments/adjustmentService/types/""
                  xmlns:rec=""http://xmlns.oracle.com/apps/financials/receivables/transactions/adjustments/adjustmentService/"">
  <soapenv:Header/>
  <soapenv:Body>
    <typ:createAdjustment>
      <typ:adjustment>
        <rec:CustomerTrxId>{item.CustomerTrxId}</rec:CustomerTrxId>
        {CreateOptionalPaymentScheduleId(paymentScheduleId)}
        <rec:Amount>{amount}</rec:Amount>
        <rec:ApplyDate>{applyDate}</rec:ApplyDate>
        <rec:GlDate>{glDate}</rec:GlDate>
        <rec:ReasonCode>{EscapeXml(item.ReasonCode)}</rec:ReasonCode>
        <rec:TransactionAdjustmentType>LINE</rec:TransactionAdjustmentType>
        <rec:ReceivablesTrxId>{receivablesTrxId}</rec:ReceivablesTrxId>
        <rec:CodeCombinationId>{_config.GetFusionAdjustmentCodeCombinationId()}</rec:CodeCombinationId>
        <rec:SetOfBooksId>{_config.GetFusionAdjustmentSetOfBooksId()}</rec:SetOfBooksId>
        <rec:OrgId>{_config.GetFusionAdjustmentOrgId()}</rec:OrgId>
        <rec:Comments>{EscapeXml(item.Remarks)}</rec:Comments>
        <rec:AdjustmentType>M</rec:AdjustmentType>
      </typ:adjustment>
    </typ:createAdjustment>
  </soapenv:Body>
</soapenv:Envelope>";
		}

		private static string CreateOptionalPaymentScheduleId(long? paymentScheduleId) =>
			paymentScheduleId.HasValue ? $"<rec:PaymentScheduleId>{paymentScheduleId.Value}</rec:PaymentScheduleId>" : string.Empty;

		private static string EscapeXml(string? value) => SecurityElement.Escape(value ?? string.Empty) ?? string.Empty;

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
