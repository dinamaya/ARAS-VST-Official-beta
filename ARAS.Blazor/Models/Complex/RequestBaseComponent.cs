using ARAS.Blazor.App_Code.Globals;
using ARAS.Blazor.Models.DTOs;
using ARAS.Blazor.Repositories.Interfaces;
using ARAS.Blazor.Services.Interfaces;
using Microsoft.AspNetCore.Components;

namespace ARAS.Blazor.Models.Complex
{
	public class RequestBaseComponent<TRow> : ComponentBase where TRow : AdjustmentRow
	{
		[Inject] protected ISearchOptionService SearchOptionService { get; set; }
		[Inject] protected IAuthService AuthService { get; set; }

		protected IList<NoteRowDto> Notes { get; set; }

		protected InvoiceDetailsDto SearchedInvoice { get; set; }
		protected RequestAuditDto RequestAudit { get; set; }

		protected bool IsOnSearch { get; private set; }
		protected bool IsLoading { get; private set; }
		protected string AdjustmentActivity { get; set; }

		protected virtual async Task OnSubmit()
		{
			Guards.ThrowInvalidOperationIf(IsLoading, "Can't submit request while search operation is ongoing");
		}

		protected virtual async Task SubmitRequest() => await Task.Delay(0);
		protected void OnSearchToggle(bool value) => IsOnSearch = value;
		protected void OnLoadingChanged(bool value) => IsLoading = value;
	}
}
