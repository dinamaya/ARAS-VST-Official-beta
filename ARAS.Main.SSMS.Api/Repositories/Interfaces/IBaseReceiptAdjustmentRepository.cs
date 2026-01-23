using ARAS.Main.SSMS.Api.Models.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ARAS.Main.SSMS.Api.Repositories.Interfaces
{
    public interface IBaseReceiptAdjustmentRepository : ICreateReceiptStatusRepository<BaseReceiptAdjustmentCreateDto, string>
	{
	}
}
