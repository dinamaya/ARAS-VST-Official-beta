using ARAS.Main.SSMS.Api.Models.Dtos;

namespace ARAS.Main.SSMS.Api.Repositories.Interfaces
{
	public interface ICreateStatusRepository
	{
		Task CreateApproveTransaction(RequestUpdateDto data, string createdBy);
		Task CreateValidateTransaction(long requestId, string createdBy);
		Task CreateDeclineTransaction(CreateDeclineDto createDecline, string createdBy);
		Task CreateRejectTransaction(long requestId, string createdBy);
	}
}
