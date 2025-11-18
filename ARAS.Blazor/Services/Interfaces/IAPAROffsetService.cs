using ARAS.Blazor.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ARAS.Blazor.Services.Interfaces
{
    public interface IAPAROffsetService
    {
        Task Create(IEnumerable<APAROffsetAPRowDto> apRows, IEnumerable<APAROffsetARRowDto> arRows, IEnumerable<NoteRowDto> notes);
    }
}
