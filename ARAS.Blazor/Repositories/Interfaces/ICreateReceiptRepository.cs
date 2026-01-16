using ARAS.Blazor.Models.DTOs;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ARAS.Blazor.Repositories.Interfaces
{
	/// <summary>
	/// Sends a Create / Update request to the server by converting the of Adjustment DTO to Creation Request Adjustment DTO
	/// </summary>
	/// <typeparam name="TCreate">Pass the Adjustment DTO</typeparam>
	public interface ICreateReceiptRepository<TCreate>
    {
		Task Create(TCreate row, IEnumerable<NoteRowDto> notes);
		Task Update(long requestId, TCreate row, IEnumerable<NoteRowDto> notes);
	}
}
