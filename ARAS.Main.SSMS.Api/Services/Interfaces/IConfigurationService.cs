namespace ARAS.Main.SSMS.Api.Services.Interfaces
{
	public interface IConfigurationService
	{
		string GetFrontendBaseUrl(string? route = "");
	}
}
