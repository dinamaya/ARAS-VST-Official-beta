namespace ARAS.Blazor.Models.DTOs
{
	public class SRAutoNetRemarksDto
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		public string CNRef { get; set; } = string.Empty;
		public double CNAmt { get; set; } = 0.00d;
		public double WT { get; set; } = 0.00d;

		public bool IsEmpty() => string.IsNullOrEmpty(CNRef) && CNAmt == 0 && WT == 0;
		public bool HasEmpty() => string.IsNullOrEmpty(CNRef) || CNAmt == 0 || WT ==0;
	}
}
