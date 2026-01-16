using Microsoft.AspNetCore.Http.HttpResults;

namespace ARAS.Blazor.Services.Interfaces
{
	/// <summary>
	/// Contains the template of the Methods not the actual implementation.
	/// Use this to map the different Receipt Adjustment Services.
	/// </summary>
	/// <typeparam name="TCreate"></typeparam>
	public interface IBaseReceiptAdjustmentService<TCreate> : 
        IBaseReceiptAdjustmentCreatorService<TCreate>
	{

    }
}
