using ARAS.Blazor.Models.DTOs;
using ARAS.Blazor.Repositories.Interfaces;

namespace ARAS.Blazor.Services.Interfaces
{
    public interface IARInvoiceOffsettingService
    {
        Task Create(IEnumerable<ARInvoiceOffsettingRowDto> arRows, IEnumerable<ARInvoiceOffsettingRowDto> cnRows, IEnumerable<NoteRowDto> notes);
        Task Update(long requestId, IEnumerable<ARInvoiceOffsettingRowDto> arRows, IEnumerable<ARInvoiceOffsettingRowDto> cnRows, IEnumerable<NoteRowDto> notes);
		Task<IEnumerable<ARInvoiceOffsettingRowDto>> GetAdjustments(long requestId);
	}
}
