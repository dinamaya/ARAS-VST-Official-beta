using ARAS.Main.SSMS.Api.Models.Dtos;

namespace ARAS.Main.SSMS.Api.Services.Interfaces
{
	public interface IAdjustmentService
	{
		Task Approve(IEnumerable<long> requestIds, string createdBy);
	}
}
