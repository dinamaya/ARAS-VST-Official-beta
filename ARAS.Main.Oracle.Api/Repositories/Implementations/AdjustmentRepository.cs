using ARAS.Main.Oracle.Api.App_Code.Globals.Constants;
using ARAS.Main.Oracle.Api.Context;
using ARAS.Main.Oracle.Api.Factories.Interfaces;
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
	}
}
