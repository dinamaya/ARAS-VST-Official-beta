namespace ARAS.Blazor.Services.Interfaces
{
	public interface ISearchOptionService
	{
		Task<IEnumerable<string>> GetReasonCodes();
	}
}
