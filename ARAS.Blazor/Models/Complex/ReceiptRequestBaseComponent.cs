namespace ARAS.Blazor.Models.Complex
{
    public class ReceiptRequestBaseComponent<TRow> : RequestBaseComponent<TRow> where TRow : AdjustmentRow
	{
        public TRow Adjustment { get; set; }
	}
}
