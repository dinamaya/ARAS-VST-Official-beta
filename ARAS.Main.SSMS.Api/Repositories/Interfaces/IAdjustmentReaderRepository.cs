namespace ARAS.Main.SSMS.Api.Repositories.Interfaces
{
	public interface IAdjustmentReaderRepository<TRow>
	{
		Task<IEnumerable<TRow>> GetAdjustmentsByRequestId(long requestId);
	}
}
