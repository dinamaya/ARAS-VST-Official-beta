namespace ARAS.Blazor.Services.Interfaces
{
	public interface IQueryService
	{
		string GetValue(string key);
		string GetCurrentUrl();
		bool KeyHasValueOf(string key, string targetValue);
	}
}
