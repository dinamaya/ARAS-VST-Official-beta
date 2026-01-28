using ARAS.Blazor.Models.DTOs;
using ARAS.Blazor.Repositories.Interfaces;

namespace ARAS.Blazor.Services.Interfaces
{
    public interface IARInvoiceOffsettingService
    {
        Task Create(IEnumerable<ARInvoiceOffsettingRowDto> arRows, IEnumerable<ARInvoiceOffsettingCNDetailsDto> cnRows);
        Task Update(long requestId, IEnumerable<ARInvoiceOffsettingRowDto> rows, IEnumerable<NoteRowDto> notes);

	}
}
