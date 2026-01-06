using ARAS.Blazor.Components.Pages.Approvals.BankCharge;
using ARAS.Blazor.Components.Shared.Modals;
using ARAS.Blazor.Components.Shared.Summaries;
using ARAS.Blazor.Models.Complex;
using ARAS.Blazor.Models.DTOs;
using Radzen;
using Radzen.Blazor;

namespace ARAS.Blazor.App_Code.Globals.Constants
{
	public class ModalTemplates
	{
		public static class Variant
		{
			public static DialogOptions XSMALL = new DialogOptions() { Width = "700px", Height = "auto" };
			public static DialogOptions SMALL = new DialogOptions() { Width = "800px", Height = "auto" };
			public static DialogOptions MEDIUM = new DialogOptions() { Width = "1000px", Height = "auto" };
			public static DialogOptions WIDE = new DialogOptions() { Width = "1500px", Height = "auto" };
		}

		public static class Add
		{

			public static async Task Adjustment<TRow>(
				DialogService dialogService,
				string title,
				IEnumerable<TRow> rows,
				IEnumerable<string> reasonCodes,
				InvoiceDetailsDto invoiceDetails,
				RadzenDataGrid<TRow> grid,
				Func<double, string, string, InvoiceDetailsDto, TRow> createRowCallBack
			)
				where TRow : AdjustmentRow
			{
				await dialogService.OpenAsync<AddAdjustmentModal<TRow>>(
					$"Add {title} Adjustment",
					new Dictionary<string, object>()
					{
						{ "Title", title },
						{ "Rows", rows },
						{ "ReasonCodes", reasonCodes },
						{ "InvoiceDetails", invoiceDetails },
						{ "Grid", grid },
						{ "OnAdjustmentRowCreate", createRowCallBack }
					},
					Variant.SMALL
				);
			}

			public static async Task CashDiscount(
				DialogService dialogService,
				RadzenDataGrid<CashDiscountRowDto> grid,
				IEnumerable<CashDiscountRowDto> rows,
				IEnumerable<string> reasonCodes,
				InvoiceDetailsDto invoiceDetails,
				string adjustmentActivity
			)
			{
				await dialogService.OpenAsync<AddCashDiscountModal>(
					"Add Cash Discount Adjustment",
					new Dictionary<string, object>()
					{
						{ nameof(AddCashDiscountModal.ReasonCodes), reasonCodes },
						{ nameof(AddCashDiscountModal.Rows), rows },
						{ nameof(AddCashDiscountModal.Grid), grid },
						{ nameof(AddCashDiscountModal.InvoiceDetails), invoiceDetails },
						{ nameof(AddCashDiscountModal.AdjustmentActivity), adjustmentActivity }
					},
					Variant.SMALL
				);
			}
			public static async Task SRAutoNet(
				DialogService dialogService,
				RadzenDataGrid<SRAutoNetRowDto> grid,
				IEnumerable<SRAutoNetRowDto> rows,
				IEnumerable<string> reasonCodes,
				InvoiceDetailsDto invoiceDetails
			)
			{
				await dialogService.OpenAsync<AddSRAutoNetModal>(
					"Add Write Off Sales Return Auto Net Adjustment",
					new Dictionary<string, object>()
					{
						{ nameof(AddSRAutoNetModal.ReasonCodes), reasonCodes },
						{ nameof(AddSRAutoNetModal.Rows), rows },
						{ nameof(AddSRAutoNetModal.Grid), grid },
						{ nameof(AddSRAutoNetModal.InvoiceDetails), invoiceDetails },
					},
					Variant.WIDE
				);
			}

			public static async Task BankCharge(
				DialogService dialogService,
				IEnumerable<BankChargeRowDto> rows,
				IEnumerable<string> reasonCodes,
				RadzenDataGrid<BankChargeRowDto> grid,
				InvoiceDetailsDto invoiceDetails,
				string adjustmentActivity
			)
			{
				await Adjustment(
					dialogService, 
					"Bank Charge", 
					rows, 
					reasonCodes, 
					invoiceDetails, 
					grid,
					(amt, remarks, reason, inv) => new BankChargeRowDto(amt, remarks, reason, adjustmentActivity, inv)
				);
			}

			public static async Task SmallAmount(
				DialogService dialogService,
				IEnumerable<SmallAmountRowDto> rows,
				IEnumerable<string> reasonCodes,
				RadzenDataGrid<SmallAmountRowDto> grid,
				InvoiceDetailsDto invoiceDetails,
				string adjustmentActivity
			)
			{
				await Adjustment(
					dialogService, 
					"Small Amount", 
					rows, 
					reasonCodes, 
					invoiceDetails,
					grid,
					(amt, remarks, reason, inv) => new SmallAmountRowDto(amt, remarks, reason, adjustmentActivity, inv)
				);
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
					$"Edit {title} Adjustment",
					new Dictionary<string, object>()
					{
					{  "InvoiceDetails", row },
					{  "Rows", rows },
					{  "ReasonCodes", reasonCodes },
					{  "Grid", grid},
					{  "OnValidate", validateCallback },
					},
					Variant.SMALL
				);
			}
		}

		public static class UpdateStatus
		{

			public static async Task Decline(
				DialogService dialogService,
				Func<string, Task> submitCallBack
			)
			{
				await dialogService.OpenAsync<StatusUpdateAdjustmentModal>(
					"Decline Request",
					new Dictionary<string, object>()
					{
						{ nameof(StatusUpdateAdjustmentModal.Message), "Are you sure you want to \"DECLINE\" this request?" },
						{ nameof(StatusUpdateAdjustmentModal.SubmitButtonTitle), "Decline" },
						{ nameof(StatusUpdateAdjustmentModal.OnSubmit), submitCallBack}
					},
					Variant.XSMALL
				);
			}

			public static async Task Reject(
				DialogService dialogService,
				Func<string, Task> submitCallBack
			)
			{
				await dialogService.OpenAsync<StatusUpdateAdjustmentModal>(
					"Reject Request",
					new Dictionary<string, object>()
					{
						{ nameof(StatusUpdateAdjustmentModal.Message), "Are you sure you want to \"REJECT\" this request?" },
						{ nameof(StatusUpdateAdjustmentModal.SubmitButtonTitle), "Reject" },
						{ nameof(StatusUpdateAdjustmentModal.OnSubmit), submitCallBack}
					},
					Variant.XSMALL
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
					Variant.WIDE
				);
			}

			public static async Task SRAutoNet(
				DialogService dialogService,
				IEnumerable<SRAutoNetRowDto> rows,
				RequestAuditDto requestAudit,
				Func<Task> submitCallBack
			)
			{
				await dialogService.OpenAsync<SRAutoNetModalSummary>(
					"Submit Cash Discount Adjustment",
					new Dictionary<string, object>()
					{
						{ "Rows", rows },
						{ "RequestAudit", requestAudit },
						{ "OnSubmit", submitCallBack },
						{ "SucccessSubmissionRoute", "/requests/sr-auto-net" },
					},
					Variant.WIDE
				);
			}

			public static async Task Adjustment(
				DialogService dialogService,
				string title,
				IEnumerable<AdjustmentRow> rows,
				RequestAuditDto requestAudit,
				string submissionRoute,
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
						{ "SucccessSubmissionRoute", $"/{submissionRoute}" },
					},
					Variant.WIDE
				);
			}
		}
	}
}
