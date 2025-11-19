namespace ARAS.Main.SSMS.Api.Models.Dtos
{
	public record AdjustmentRequestCreationDto<TAdjustments>(IEnumerable<TAdjustments> Adjustments, IEnumerable<string> ToEmail);
}
