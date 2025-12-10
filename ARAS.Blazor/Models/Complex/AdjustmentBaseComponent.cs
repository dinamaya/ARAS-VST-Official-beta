using ARAS.Blazor.Models.DTOs;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace ARAS.Blazor.Models.Complex
{
	public class AdjustmentBaseComponent<TRow> : ComponentBase
		where TRow : AdjustmentRow
	{
		[Parameter] public string Title { get; set; }
		[Parameter] public IEnumerable<string> ReasonCodes { get; set; }
		[Parameter] public IList<TRow> Rows { get; set; }
		[Parameter] public InvoiceDetailsDto InvoiceDetails { get; set; }
		[Parameter] public Func<double, string, string, InvoiceDetailsDto, TRow> OnAdjustmentRowCreate { get; set; }
		[Parameter] public RadzenDataGrid<TRow> Grid { get; set; }

		protected IEnumerable<string> invoices = new List<string>();
		protected string adjustmentAmount = string.Empty;
		protected string remarks = string.Empty;
		protected string reasonCode = string.Empty;

		protected virtual async Task OnAdd(DialogService dialogService, TRow row)
		{
			Rows.Add(row);

			await Grid.Reload();
			dialogService.Close(true);
		}
	}
}
