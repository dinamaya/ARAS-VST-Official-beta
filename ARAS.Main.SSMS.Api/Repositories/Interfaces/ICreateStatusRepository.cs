namespace ARAS.Main.SSMS.Api.Repositories.Interfaces
{
	public interface ICreateStatusRepository
	{
		Task CreateApproveTransaction(long requestId, string createdBy);
		Task CreateValidateTransaction(long requestId, string createdBy);
		Task CreateDeclineTransaction(long requestId, string createdBy);
		Task CreateRejectTransaction(long requestId, string createdBy);
	}
}
