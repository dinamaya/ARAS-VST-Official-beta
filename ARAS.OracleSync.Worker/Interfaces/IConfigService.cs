namespace ARAS.OracleSync.Worker.Interfaces
{
	public interface IConfigService
	{
		string GetOracleAdjustmentsApiUrl(string route = null);
		string GetRequestsApiUrl(string route = null);
	}
}
