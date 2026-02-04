namespace ARAS.OracleSync.Worker.Interfaces
{
	public interface IConfigService
	{
		string GetApprovalsUrl(string route = null);

		string GetSSMSAdjustmentsApiUrl(string route = null);
		string GetOracleAdjustmentsApiUrl(string route = null);
		string GetRequestsApiUrl(string route = null);
		int GetRefreshTime();
	}
}
