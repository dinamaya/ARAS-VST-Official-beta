using ARAS.Main.Oracle.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.Models.Entities;

namespace ARAS.Main.SSMS.Api.Repositories.Interfaces
{
	public interface IAdjustmentRepository : ICreateRepository<AdjustmentCreateDto, long>
	{
		Task<IEnumerable<Adjustment>> GetByRequestId(long requestId);
		Task DeactivateDetails(long id);
		Task DeactivateAllByRequestId(long requestId);
		Task<string> GenerateReferenceNumber(string groupCode, string adjustmentTypeCode);
	}
}
