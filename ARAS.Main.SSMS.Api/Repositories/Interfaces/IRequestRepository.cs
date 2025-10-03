namespace ARAS.Main.SSMS.Api.Repositories.Interfaces
{
	public interface IRequestRepository
	{
		Task<bool> IsApprovable(long requestId);
		Task<bool> IsValidatable (long requestId);
	}
}
