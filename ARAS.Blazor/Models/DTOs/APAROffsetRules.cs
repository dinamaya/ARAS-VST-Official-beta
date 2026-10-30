namespace ARAS.Blazor.Models.DTOs
{
	/// <summary>
	/// Business rules for AP-AR offset requests that apply on the request pages.
	/// Company practice: each request offsets exactly one AR invoice.
	/// </summary>
	public static class APAROffsetRules
	{
		public const string SingleArInvoiceHint = "Only one AR invoice per request.";

		public const string SingleArInvoiceMessage =
			"An AP-AR offset request can have only one AR invoice. " +
			"Remove the current AR invoice to choose a different one.";

		public const string TooManyArInvoicesMessage =
			"An AP-AR offset request can have only one AR invoice. " +
			"Please remove the extra AR invoices before submitting.";

		/// <summary>AR invoice rows in the AR group (the write-off row is not counted).</summary>
		public static int CountArInvoices(IEnumerable<APAROffsetARRowDto>? arRows) =>
			(arRows ?? []).Count(r => r is not null && r.RowType != ARRowType.Adjustment);
	}
}
