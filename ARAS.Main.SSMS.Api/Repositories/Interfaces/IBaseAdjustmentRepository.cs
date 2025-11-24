using ARAS.Main.SSMS.Api.Models.Dtos;

namespace ARAS.Main.SSMS.Api.Repositories.Interfaces
{
	public interface IBaseAdjustmentRepository<TCreate>
	{
		Task<long> Create(
			RequestCreationDto<AdjustmentRequestCreationDto<TCreate>> data,
			string createdBy,
			string adjustmentTypeCode,
			Func<TCreate, string, long, Task> createInvoiceCallBack);
		Task<long> Update(
			long requestId, 
			RequestCreationDto<AdjustmentRequestCreationDto<TCreate>> data, 
			string modifiedBy,
			string adjustmentTypeCode,
			string adjustmentRouteName,
			Func<TCreate, string, long, Task> updateInvoiceCallBack);

		Task Approve(RequestUpdateDto data, string createdBy, string adjustmentTypeName);
		Task Validate(RequestUpdateDto data, string createdBy, string adjustmentTypeName);
		Task Decline(NegateRequestDto data, string createdBy, string adjustmentTypeName);
		Task Reject(NegateRequestDto data, string createdBy, string adjustmentTypeName);
	}
}
