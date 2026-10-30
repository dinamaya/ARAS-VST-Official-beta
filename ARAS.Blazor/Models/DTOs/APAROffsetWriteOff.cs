namespace ARAS.Blazor.Models.DTOs
{
	/// <summary>
	/// Rules for the write-off row of an AP-AR offset request. The write-off has no
	/// invoice number of its own, so it is posted to Fusion against one of the
	/// request's own AR invoices, which must carry its Fusion CustomerTrxId.
	/// </summary>
	public static class APAROffsetWriteOff
	{
		/// <summary>
		/// The AR invoice the write-off will be posted against: the first AR invoice in
		/// this request that has a Fusion CustomerTrxId, otherwise the first AR invoice
		/// (which then fails <see cref="IsMatchedToFusion"/>).
		/// </summary>
		public static APAROffsetARRowDto? FindInvoice(IEnumerable<APAROffsetARRowDto>? arRows)
		{
			var invoices = (arRows ?? [])
				.Where(r => r is not null &&
					r.RowType != ARRowType.Adjustment &&
					!string.IsNullOrWhiteSpace(r.InvoiceNumber))
				.ToList();

			return invoices.FirstOrDefault(r => r.CustomerTrxId is not null) ?? invoices.FirstOrDefault();
		}

		public static bool IsMatchedToFusion(APAROffsetARRowDto? invoice) => invoice?.CustomerTrxId is not null;

		public static string NotMatchedMessage(string? invoiceNumber) =>
			$"Invoice {(string.IsNullOrWhiteSpace(invoiceNumber) ? "(no invoice number)" : invoiceNumber.Trim())} can't be used for the write-off. " +
			"It couldn't be matched to an invoice in Oracle Fusion for this customer. " +
			"Please check that the invoice exists in Fusion and is complete, then try again. " +
			"If it does, contact the system administrator.";
	}
}
