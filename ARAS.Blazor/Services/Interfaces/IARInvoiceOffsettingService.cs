using ARAS.Blazor.Models.DTOs;
using ARAS.Blazor.Repositories.Interfaces;

namespace ARAS.Blazor.Services.Interfaces
{
    public interface IARInvoiceOffsettingService : IBaseAdjustmentCommandRepository<ARInvoiceOffsettingRowDto, ARInvoiceOffsettingCreateDto, object>
    {
        Task Create(IEnumerable<ARInvoiceOffsettingRowDto> rows, IEnumerable<NoteRowDto> notes);
        Task Update(long requestId, IEnumerable<ARInvoiceOffsettingRowDto> rows, IEnumerable<NoteRowDto> notes);
        Task Approve(long requestId, IEnumerable<NoteRowDto> notes);
        Task Validate(long requestId, IEnumerable<NoteRowDto> notes);
        Task Decline(NegateRequestDto createDecline, IEnumerable<NoteRowDto> notes);
        Task Reject(NegateRequestDto createReject, IEnumerable<NoteRowDto> notes);
        Task<IEnumerable<ARInvoiceOffsettingRowDto>> GetAdjustments(long requestId);
    }
}
