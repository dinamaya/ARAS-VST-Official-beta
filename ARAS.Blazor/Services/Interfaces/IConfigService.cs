namespace ARAS.Blazor.Services.Interfaces
{
	public interface IConfigService
	{
		string GetBaseUrl(string route = null);
		string GetAuthUrl(string route = null);
		string GetAuthEmailUrl(string route = null);
		string GetOpsUrl(string route = null);

		string GetOracleInvoiceApiUrl(string route = null);
		string GetOracleAdjustmentApiUrl(string route = null);

		string GetAdjustmentsUrl(string route = null);
		string GetApprovalsUrl(string route = null);
		string GetCashDiscountsUrl(string route = null);
        string GetAPAROffsetsUrl(string route = null);
		string GetBankChargesUrl(string route = null);
		string GetDeclinesUrl(string route = null);
		string GetRejectionsUrl(string route = null);
		string GetRequestsUrl(string route = null);
		string GetTransactionsUrl(string route = null);
		string GetValidationsUrl(string route = null);
		string GetSmallAmountUrl(string route = null);
		string GetFilesUrl(string route = null);

		string GetTokenName();
		string GetTokenDomainName();

		bool IsOnTestRequest();
	}
}
