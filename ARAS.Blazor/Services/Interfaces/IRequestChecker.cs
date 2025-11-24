namespace ARAS.Blazor.Services.Interfaces
{
	public interface IRequestChecker
	{
		Task<bool> IsApprovable(long requestId);
		Task<bool> IsValidatable(long requestId);
	}
}
