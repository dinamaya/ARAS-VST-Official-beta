using ARAS.Blazor.Models.DTOs;

namespace ARAS.Blazor.Models.Complex
{
	public class SummaryDataTableContext<TRow>
	where TRow : AdjustmentRow
	{
		public IEnumerable<TRow> Rows { get; init; } = [];
		public RequestAuditDto RequestAudit { get; init; } = default!;
	}
}
