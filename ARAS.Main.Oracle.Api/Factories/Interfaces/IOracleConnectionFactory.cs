using Oracle.ManagedDataAccess.Client;

namespace ARAS.Main.Oracle.Api.Factories.Interfaces
{
	public interface IOracleConnectionFactory
	{
		Task<OracleConnection> OpenWithoutPolicyAsync();
		Task<OracleConnection> OpenWithPolicyContextAsync();
	}
}
