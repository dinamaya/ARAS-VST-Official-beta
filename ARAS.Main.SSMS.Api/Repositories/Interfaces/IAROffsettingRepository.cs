using ARAS.Main.SSMS.Api.Models.Dtos;
namespace ARAS.Main.SSMS.Api.Repositories.Interfaces
{
    public interface IAROffsettingRepository :  ICreateReceiptStatusRepository<ARInvoiceOffsettingCreateDto, long>
    {
		Task<long> Update(long requestId, RequestCreationDto<IEnumerable<ARInvoiceOffsettingCreateDto>> data, string modifiedBy);
		Task<IEnumerable<ARInvoiceOffsettingRowDto>> GetAdjustmentsByRequestId(long requestId);
	}
}
