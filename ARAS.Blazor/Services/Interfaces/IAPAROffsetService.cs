using ARAS.Blazor.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ARAS.Blazor.Services.Interfaces
{
    public interface IAPAROffsetService
    {
        Task Create(IEnumerable<APAROffsetAPRowDto> apRows, IEnumerable<APAROffsetARRowDto> arRows, IEnumerable<NoteRowDto> notes);
        Task<IEnumerable<TransactionRequestRowDto>> GetSubmissions();
        Task<TransactionRequestRowDto> GetRequestDetails(long requestId);
        Task<Tuple<IEnumerable<APAROffsetAPRowDto>, IEnumerable<APAROffsetARRowDto>>> GetAdjustments(long requestId);
        Task Update(long requestId, IEnumerable<APAROffsetAPRowDto> apRows, IEnumerable<APAROffsetARRowDto> arRows, IEnumerable<NoteRowDto> notes);
        Task Decline(NegateRequestDto createDecline, IEnumerable<NoteRowDto> notes);
    }
}
