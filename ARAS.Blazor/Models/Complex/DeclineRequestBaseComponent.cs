using ARAS.Blazor.Models.DTOs;
using ARAS.Blazor.Repositories.Interfaces;
using ARAS.Blazor.Services.Interfaces;
using Microsoft.AspNetCore.Components;

namespace ARAS.Blazor.Models.Complex
{
	public class DeclineRequestBaseComponent<TRow, TAdjustmentService> :
		RequestBaseComponent<TRow, TAdjustmentService>
			where TRow : AdjustmentRow
			where TAdjustmentService : ICreateStatusRepository<TRow>, IAdjustmentReaderRepository<TRow>
	{
		[Inject] protected INoteService NoteService { get; set; }
		[Inject] protected IRequestService RequestService { get; set; }

		[Parameter] public long RequestId { get; set; }


		protected override async Task OnInitializedAsync()
		{
			await base.OnInitializedAsync();
			Adjustments = (await AdjustmentService.GetAdjustments(RequestId)).ToList();
			Notes = await NoteService.GetRows(RequestId);
			var request = await RequestService.GetRequestDetails(RequestId);
			ReasonCodes = await SearchOptionService.GetReasonCodes();
			RequestAudit = new RequestAuditDto(request);
		}
	}
}
