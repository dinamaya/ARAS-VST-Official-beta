namespace ARAS.Blazor.Repositories.Interfaces
{
	public interface IAdjustmentReaderRepository<TRow>
	{
		Task<string> GetActivityByCode();
		Task<IEnumerable<TRow>> GetAdjustments(long requestId);
	}
}
