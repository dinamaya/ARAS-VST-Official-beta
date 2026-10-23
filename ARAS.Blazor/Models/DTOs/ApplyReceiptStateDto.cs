namespace ARAS.Blazor.Models.DTOs
{
	public class ApplyReceiptStateDto
	{
		public bool IsEnabled { get; set; }
		public IList<ReceiptInvoiceApplicationSelectionDto> SelectedApplications { get; set; } = [];
		public double TotalAppliedAmount => SelectedApplications.Sum(x => x.AppliedAmount);
	}
}
