using ARAS.Main.SSMS.Api.Models.Dtos;

namespace ARAS.Main.SSMS.Api.Repositories.Interfaces
{
	public interface ICreateStatusRepository
	{
		Task CreateApproveTransaction(RequestUpdateDto data, string createdBy);
		Task CreateValidateTransaction(RequestUpdateDto data, string createdBy);
		Task CreateDeclineTransaction(NegateRequestDto createDecline, string createdBy);
		Task CreateRejectTransaction(NegateRequestDto createDecline, string createdBy);
	}
}
