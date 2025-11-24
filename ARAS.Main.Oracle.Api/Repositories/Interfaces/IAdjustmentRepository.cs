namespace ARAS.Main.Oracle.Api.Repositories.Interfaces
{
	public interface IAdjustmentRepository
	{
		Task<IEnumerable<string>> GetReasonCodes();
	}
}
