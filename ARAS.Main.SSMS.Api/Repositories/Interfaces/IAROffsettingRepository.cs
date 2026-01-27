using ARAS.Main.SSMS.Api.Models.Dtos;
namespace ARAS.Main.SSMS.Api.Repositories.Interfaces
{
    public interface IAROffsettingRepository :  ICreateReceiptStatusRepository<ARInvoiceOffsettingCreateDto, long>
    {
    }
}
