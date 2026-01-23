using ARAS.Blazor.Models.DTOs;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ARAS.Blazor.Services.Interfaces
{
	/// <summary>
	/// This is use in the actual implementation of the Receipt Adjustment Services.
	/// </summary>
	/// <typeparam name="TCreate">Pass the Adjustment DTO</typeparam>
	public interface IBaseReceiptAdjustmentCreatorService<TCreate>
    {
		Task Create(TCreate data, IEnumerable<NoteRowDto> notes, string route);
		Task Update(long requestId, TCreate data, IEnumerable<NoteRowDto> notes, string route);
	}
}
