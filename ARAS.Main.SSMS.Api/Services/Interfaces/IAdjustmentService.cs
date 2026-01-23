using ARAS.Main.SSMS.Api.Models.Dtos;

namespace ARAS.Main.SSMS.Api.Services.Interfaces
{
	public interface IAdjustmentService
	{
		Task Approve(IEnumerable<long> requestIds, string createdBy);
		Task Decline(IEnumerable<long> requestIds, string createdBy);
		Task Reject(IEnumerable<long> requestIds, string createdBy);
	}
}
