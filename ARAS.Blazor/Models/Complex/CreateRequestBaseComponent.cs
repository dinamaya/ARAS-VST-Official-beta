using ARAS.Blazor.App_Code.Globals;
using ARAS.Blazor.App_Code.Globals.Constants;
using ARAS.Blazor.Components.Shared.DataTables;
using ARAS.Blazor.Models.DTOs;
using ARAS.Blazor.Repositories.Interfaces;
using ARAS.Blazor.Services.Implementations;
using ARAS.Blazor.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Radzen;

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

			RequestAudit = new()
			{
				Requestor = await AuthService.GetLastFirstName()
			};
		}
	}
}
