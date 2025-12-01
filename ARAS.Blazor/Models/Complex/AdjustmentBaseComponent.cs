using ARAS.Blazor.Components.Shared.Modals;
using ARAS.Blazor.Models.DTOs;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace ARAS.Blazor.Models.Complex
{
	public abstract class AdjustmentBaseComponent : ComponentBase
	{
		//protected abstract Task OpenAdd(InvoiceDetailsDto invoiceDetails);
		//protected abstract Task OpenEdit(SmallAmountRowDto data);
		//protected abstract Task OnSubmit();

		//public async Task SubmitRequest()
		//{
		//	// await BankChargeService.Create(Adjustments, Notes);
		//}

		//private void HandleSearchChanged(bool value)
		//{
		//	isOnSearch = value;
		//}

		//private void HandleLoadingChanged(bool value)
		//{
		//	isLoading = value;
		//}

		//private async Task OnAdd(double amount, string remarks, string reasonCode, InvoiceDetailsDto details)
		//{
		//	var row = new SmallAmountRowDto(amount, remarks, reasonCode, details);
		//	Adjustments.Add(row);
		//	await adjustmentGrid.Reload();
		//}

		//private async Task OnEdit(double amount, string remarks, string reasonCode, SmallAmountRowDto details)
		//{
		//	var data = Adjustments.FirstOrDefault(r => r.Id == details.Id) ?? throw new Exception("Data is not found in the Small Amount adjustment table");
		//	data.SetValues((float)amount, remarks, reasonCode);

		//	await adjustmentGrid.Reload();
		//}

		//private async Task OnDelete(SmallAmountRowDto details)
		//{
		//	Adjustments.Remove(details);
		//	await adjustmentGrid.Reload();
		//}

		//private async Task OnValidate(double amount, string remarks, string reasonCode, InvoiceDetailsDto details)
		//{
		//	var adjustments = Adjustments.Where(a => a.InvoiceNumber.Trim() == details.InvoiceNumber);

		//	if (!adjustments.Any()) return;

		//	var sum = adjustments.Sum(a => a.AdjustmentAmount);

		//	Guards.ThrowInvalidOperationIf(sum > details.InvoiceAmount, "The sum of the adjustment amount already exceeded the invoice amount");
		//}
	}
}
