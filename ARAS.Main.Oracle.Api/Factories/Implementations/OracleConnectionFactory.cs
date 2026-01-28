using ARAS.Main.Oracle.Api.Factories.Interfaces;
using ARAS.Main.Oracle.Api.Services.Interfaces;
using Dapper;
using Oracle.ManagedDataAccess.Client;

namespace ARAS.Main.Oracle.Api.Factories.Implementations
{
	public class OracleConnectionFactory : IOracleConnectionFactory
	{
		private readonly IConfiguration config;
		private readonly IConfigurationService configService;

		public OracleConnectionFactory(IConfiguration config, IConfigurationService configService)
		{
			this.config = config;
			this.configService = configService;
		}

		public Task<OracleConnection> OpenContextAsync() => configService.IsOntest() ? OpenWithPolicyContextAsync() : OpenWithoutPolicyAsync();

		public async Task<OracleConnection> OpenWithoutPolicyAsync()
		{
			var conn = new OracleConnection(config.GetConnectionString("MainDbContext"));
			await conn.OpenAsync();
			return conn;
		}

		public async Task<OracleConnection> OpenWithPolicyContextAsync()
		{
			var conn = new OracleConnection(config.GetConnectionString("MainDbContext"));
			await conn.OpenAsync();

			await conn.ExecuteAsync(
				"BEGIN mo_global.set_policy_context(:p1, :p2); END;",
				new { p1 = "S", p2 = 101 }
			);

			return conn;
		}
	}
}
