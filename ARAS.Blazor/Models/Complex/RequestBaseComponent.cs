using ARAS.Blazor.App_Code.Globals;
using ARAS.Blazor.Models.DTOs;
using ARAS.Blazor.Repositories.Interfaces;
using ARAS.Blazor.Services.Interfaces;
using Microsoft.AspNetCore.Components;

namespace ARAS.Blazor.Models.Complex
{
	public class RequestBaseComponent<TRow, TAdjustmentService> :
		ComponentBase
			where TRow : AdjustmentRow
			where TAdjustmentService : ICreateStatusRepository<TRow>, IAdjustmentReaderRepository<TRow>
	{
		[Inject] protected ISearchOptionService SearchOptionService { get; set; }
		[Inject] protected IAuthService AuthService { get; set; }
		[Inject] protected TAdjustmentService AdjustmentService { get; set; }


		protected IList<TRow> Adjustments { get; set; }
		protected IList<NoteRowDto> Notes { get; set; }
		protected IEnumerable<string> ReasonCodes { get; set; }

		protected InvoiceDetailsDto SearchedInvoice { get; set; }
		protected RequestAuditDto RequestAudit { get; set; }

		protected bool IsOnSearch { get; private set; }
		protected bool IsLoading { get; private set; }
		protected string AdjustmentActivity { get; set; }

		protected virtual async Task OnSubmit()
		{
			Guards.ThrowInvalidOperationIf(!Adjustments.Any(), "Can't submit request because there are no adjustment in the current request");
			Guards.ThrowInvalidOperationIf(IsLoading, "Can't submit request while search operation is ongoing");
		}

		protected virtual async Task SubmitRequest() => await AdjustmentService.Create(Adjustments, Notes);
		protected void OnSearchToggle(bool value) => IsOnSearch = value;
		protected void OnLoadingChanged(bool value) => IsLoading = value;
	}
}
