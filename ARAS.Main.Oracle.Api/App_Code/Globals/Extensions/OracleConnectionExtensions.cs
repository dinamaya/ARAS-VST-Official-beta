using Dapper;
using Oracle.ManagedDataAccess.Client;

namespace ARAS.Main.Oracle.Api.App_Code.Globals.Extensions
{
	public static class OracleConnectionExtensions
	{
		public static async Task<OracleConnection> OpenWithPolicyContextAsync(
			this OracleConnection conn, string responsibility = "S", int orgId = 101)
		{
			await conn.OpenAsync();
			await conn.ExecuteAsync(
				"BEGIN mo_global.set_policy_context(:resp, :org); END;",
				new { resp = responsibility, org = orgId }
			);

			return conn;
		}
	}
}
