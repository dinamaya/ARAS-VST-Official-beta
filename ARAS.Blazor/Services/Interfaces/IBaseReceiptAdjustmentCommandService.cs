using ARAS.Blazor.Repositories.Interfaces;

namespace ARAS.Blazor.Services.Interfaces
{
    public interface IBaseReceiptAdjustmentCommandService<TCreate, TRow> :
		ICreateReceiptRepository<TRow>
    {
    }
}
