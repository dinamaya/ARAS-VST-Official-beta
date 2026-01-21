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
	}
}
