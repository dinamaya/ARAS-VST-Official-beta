using ARAS.Main.SSMS.Api.Models.Dtos;

namespace ARAS.Main.SSMS.Api.Services.Interfaces
{
	public interface IAdjustmentService
	{
		Task Approve(RequestUpdateDto data, string createdBy, string adjustmentTypeCode);
		Task Validate(RequestUpdateDto data, string createdBy, string adjustmentTypeCode);
		Task Decline(NegateRequestDto createDecline, string createdBy, string adjustmentTypeCode);
		Task Reject(NegateRequestDto createDecline, string createdBy, string adjustmentTypeCode);
	}
}
