namespace ARAS.Blazor.Models.Complex
{
    public class CreateReceiptRequestBaseComponent<TRow> : ReceiptRequestBaseComponent<TRow> where TRow : AdjustmentRow
	{
		protected override async Task OnInitializedAsync()
		{
			await base.OnInitializedAsync();
			Notes = [];
			SearchedInvoice = new();
			AdjustmentActivity = null;
			RequestAudit = new()
			{
				Requestor = await AuthService.GetLastFirstName()
			};
		}
    }
}
