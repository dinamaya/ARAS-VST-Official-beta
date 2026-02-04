using ARAS.Main.SSMS.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.Models.Entities;
using ARAS.Main.SSMS.Api.Models.SQLVIews;

namespace ARAS.Main.SSMS.Api.Repositories.Interfaces
{
	public interface IAdjustmentRepository : 
		ICreateRepository<AdjustmentCreateDto, long>,
		ICreateMultipleRepository<AdjustmentCreateDto, long>,
		ICreateMultipleRepository<APAROffsetCreateDto, long>,
		ICreateMultipleRepository<ARIAdjustmentCreateDto, long>,
		ICreateMultipleRepository<ReasonAdjustmentCreateDto, long>,
		IUpdateRepository<ReceiptAdjustmentUpdateRequestDto, long>
	{
		Task<IEnumerable<string>> GetTypes();
		Task<IEnumerable<string>> GetReceiptTypes();
		Task<IEnumerable<string>> GetInvoiceTypes();
		Task<string> GetActivityNameByCode(string adjustmentTypeCode);
		Task<IEnumerable<Adjustment>> GetByRequestId(long requestId);
		Task<Adjustment> GetOneByRequestId(long requestId);
		Task DeactivateDetails(long id);
		Task DeactivateAllByRequestId(long requestId);
		Task DeactivateAPARByRequestId(long requestId);
		Task<string> GenerateReferenceNumber(string groupCode, string adjustmentTypeCode);
        Task<string> GenerateAPARReferenceNumber();
        Task<AdjustmentBasicInfoDto> GetAdjustmentInfoByCode(string adjustmentTypeCode);
        Task<AdjustmentBasicInfoDto> GetAdjustmentInfoByName(string adjustmentName);
		Task<IEnumerable<RequestAdjustmentsV>> GetAllByRequestIdAndCode(long requestId, string adjustmentTypeCode);
		Task<IEnumerable<long>> GetAllApproved();
	}
}
