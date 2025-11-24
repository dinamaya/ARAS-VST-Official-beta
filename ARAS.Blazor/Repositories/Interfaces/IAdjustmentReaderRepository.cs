namespace ARAS.Blazor.Repositories.Interfaces
{
	public interface IAdjustmentReaderRepository<TRow>
	{
		Task<IEnumerable<TRow>> GetAdjustments(long requestId);
	}
}
