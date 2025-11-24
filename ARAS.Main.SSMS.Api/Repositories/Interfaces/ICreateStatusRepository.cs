using ARAS.Main.SSMS.Api.Models.Dtos;

namespace ARAS.Main.SSMS.Api.Repositories.Interfaces
{
	public interface ICreateStatusRepository
	{
		Task ApproveAsync(RequestUpdateDto data, string createdBy);
		Task ValidateAsync(RequestUpdateDto data, string createdBy);
		Task DeclineAsync(NegateRequestDto createDecline, string createdBy);
		Task RejectAsync(NegateRequestDto createDecline, string createdBy);
	}
}
