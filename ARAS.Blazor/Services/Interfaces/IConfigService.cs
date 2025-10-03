namespace ARAS.Blazor.Services.Interfaces
{
	public interface IConfigService
	{
		string GetBaseApiUrl(string route = null);
		string GetAuthApiUrl(string route = null);
		string GetMainSSMSApiUrl(string route = null);
		string GetMainOracleApiUrl(string route = null);
		string GetTokenName();
		string GetTokenDomainName();
	}
}
