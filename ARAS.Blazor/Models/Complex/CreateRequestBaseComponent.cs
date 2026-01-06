using ARAS.Blazor.Repositories.Interfaces;


namespace ARAS.Blazor.Models.Complex
{
	public class CreateRequestBaseComponent<TRow, TAdjustmentService> : 
		RequestBaseComponent<TRow, TAdjustmentService>
			where TRow : AdjustmentRow
			where TAdjustmentService : ICreateStatusRepository<TRow>, IAdjustmentReaderRepository<TRow>
	{
		protected override async Task OnInitializedAsync()
		{
			await base.OnInitializedAsync();
			Adjustments = [];
			Notes = [];
			SearchedInvoice = new();
			ReasonCodes = await SearchOptionService.GetReasonCodes();
			AdjustmentActivity = await AdjustmentService.GetActivityByCode();

			RequestAudit = new()
			{
				Requestor = await AuthService.GetLastFirstName()
			};
		}
	}
}
