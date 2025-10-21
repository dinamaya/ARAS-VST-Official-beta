namespace ARAS.Blazor.Models.DTOs
{
	public record AdjustmentRequestCreationDto<TAdjustments>(IEnumerable<TAdjustments> Adjustments, IEnumerable<string> ToEmail);
}
