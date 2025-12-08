using ARAS.Blazor.Components.Pages.Approvals.BankCharge;
using ARAS.Blazor.Components.Shared.Modals;
using ARAS.Blazor.Models.Complex;
using ARAS.Blazor.Models.DTOs;
using Radzen;
using Radzen.Blazor;

namespace ARAS.Blazor.App_Code.Globals.Constants
{
	public class ModalTemplates
	{
		public static class Size
		{
			public static DialogOptions SMALL = new DialogOptions() { Width = "800px", Height = "auto" };
			public static DialogOptions WIDE = new DialogOptions() { Width = "1500px", Height = "auto" };
		}

		public static class Add
		{

			public static async Task Adjustment(
				DialogService dialogService,
				string title,
				IEnumerable<AdjustmentRow> rows,
				IEnumerable<string> reasonCodes,
				InvoiceDetailsDto invoiceDetails,
				Func<double, string, string, InvoiceDetailsDto, AdjustmentRow> createRowCallBack
			)
			{
				await dialogService.OpenAsync<AddAdjustmentModal<AdjustmentRow>>(
					$"Add {title} Adjustment",
					new Dictionary<string, object>()
					{
						{ "Title", title },
						{ "Rows", rows },
						{ "ReasonCodes", reasonCodes },
						{ "InvoiceDetails", invoiceDetails },
						{ "OnAdjustmentRowCreate", createRowCallBack }
					},
					Size.SMALL
				);
			}

			public static async Task CashDiscount(
				DialogService dialogService,
				RadzenDataGrid<CashDiscountRowDto> grid,
				IEnumerable<CashDiscountRowDto> rows,
				IEnumerable<string> reasonCodes,
				InvoiceDetailsDto invoiceDetails
			)
			{
				await dialogService.OpenAsync<AddCashDiscountModal>(
					"Add Cash Discount Adjustment",
					new Dictionary<string, object>()
					{
						{ nameof(AddCashDiscountModal.ReasonCodes), reasonCodes },
						{ nameof(AddCashDiscountModal.Rows), rows },
						{ nameof(AddCashDiscountModal.Grid), grid },
						{ nameof(AddCashDiscountModal.InvoiceDetails), invoiceDetails }
					},
					Size.SMALL
				);
			}

			public static async Task BankCharge(
				DialogService dialogService,
				IEnumerable<BankChargeRowDto> rows,
				IEnumerable<string> reasonCodes,
				InvoiceDetailsDto invoiceDetails
			)
			{
				await Adjustment(
					dialogService, 
					"Bank Charge", 
					rows, 
					reasonCodes, 
					invoiceDetails, 
					(amt, remarks, reason, inv) => new BankChargeRowDto(amt, remarks, reason, inv)
				);
			}

			public static async Task SmallAmount(
				DialogService dialogService,
				IEnumerable<SmallAmountRowDto> rows,
				IEnumerable<string> reasonCodes,
				InvoiceDetailsDto invoiceDetails
			)
			{
				await Adjustment(
					dialogService, 
					"Small Amount", 
					rows, 
					reasonCodes, 
					invoiceDetails, 
					(amt, remarks, reason, inv) => new SmallAmountRowDto(amt, remarks, reason, inv));
			}

		}
		public static class Edit<TRow> where TRow : AdjustmentRow
		{

			public static async Task Adjustment(
				DialogService dialogService,
				string title,
				TRow row,
				IList<TRow> rows,
				IEnumerable<string> reasonCodes,
				RadzenDataGrid<TRow> grid,
				Func<double, string, string, TRow, Task>? validateCallback
			)
			{
				await dialogService.OpenAsync<EditInvoiceAdjustmentModal<TRow>>(
					$"Edit {title} Charge Adjustment",
					new Dictionary<string, object>()
					{
					{  "InvoiceDetails", row },
					{  "Rows", rows },
					{  "ReasonCodes", reasonCodes },
					{  "Grid", grid},
					{  "OnValidate", validateCallback },
					},
					Size.SMALL
				);
			}
		}

		public static class Summary
		{
			public static async Task CashDiscount(
				DialogService dialogService,
				IEnumerable<CashDiscountRowDto> rows,
				RequestAuditDto requestAudit,
				Func<Task> submitCallBack
			)
			{
				await dialogService.OpenAsync<CashDiscountModalSummary>(
					"Submit Cash Discount Adjustment",
					new Dictionary<string, object>()
					{
						{ "Rows", rows },
						{ "RequestAudit", requestAudit },
						{ "OnSubmit", submitCallBack },
						{ "SucccessSubmissionRoute", "/requests/cash-discount" },
					},
					Size.WIDE
				);
			}

			public static async Task Adjustment(
				DialogService dialogService,
				string title,
				IEnumerable<AdjustmentRow> rows,
				RequestAuditDto requestAudit,
				string requestRoute,
				Func<Task> submitCallBack
			)
			{
				await dialogService.OpenAsync<RequestAdjustmentsModalSummary>(
					$"Submit {title} Adjustment",
					new Dictionary<string, object>()
					{
						{ "Rows", rows },
						{ "RequestAudit", requestAudit },
						{ "OnSubmit", submitCallBack },
						{ "SucccessSubmissionRoute", $"/requests/{requestRoute}" },
					},
					Size.WIDE
				);
			}
		}
	}
}
