namespace ARAS.Blazor.Services.Interfaces
{
    public interface IAdjustmentService
    {
        Task<IEnumerable<string>> GetAdjustmentTypes();
    }
}
