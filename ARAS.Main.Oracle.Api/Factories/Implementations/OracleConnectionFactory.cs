using ARAS.Main.Oracle.Api.Factories.Interfaces;
using Dapper;
using Oracle.ManagedDataAccess.Client;

namespace ARAS.Main.Oracle.Api.Factories.Implementations
{
	public class OracleConnectionFactory : IOracleConnectionFactory
	{
		private readonly IConfiguration config;

		public OracleConnectionFactory(IConfiguration config)
		{
			this.config = config;
		}

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
