using ARAS.Main.SSMS.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.Models.Entities;
using ARAS.Main.SSMS.Api.Models.SQLVIews;

namespace ARAS.Main.SSMS.Api.Repositories.Interfaces
{
	public interface IAdjustmentRepository : ICreateRepository<AdjustmentCreateDto, long>
	{
		Task<long> CreateAPAROffsetAdjustmentAsync(APAROffsetCreateDto data, long invoiceId, string createdBy, long requestId, string adjustmentTypeId);
		Task<IEnumerable<Adjustment>> GetByRequestId(long requestId);
		Task DeactivateDetails(long id);
		Task DeactivateAllByRequestId(long requestId);
		Task<string> GenerateReferenceNumber(string groupCode, string adjustmentTypeCode);
        Task<string> GenerateAPARReferenceNumber();
        Task<AdjustmentBasicInfoDto> GetAdjustmentInfoByCode(string adjustmentTypeCode);
		Task<IEnumerable<RequestAdjustmentsV>> GetAllByRequestIdAndCode(long requestId, string adjustmentTypeCode);
	}
}
