using ARAS.Blazor.Models.DTOs;
using ARAS.Blazor.Repositories.Interfaces;

namespace ARAS.Blazor.Services.Interfaces
{
    public interface IAdjustmentService
    {
        Task<IEnumerable<string>> GetAdjustmentTypes();
        Task<IEnumerable<string>> GetReceiptTypes();
        Task<IEnumerable<string>> GetInvoiceTypes();
	}
}
