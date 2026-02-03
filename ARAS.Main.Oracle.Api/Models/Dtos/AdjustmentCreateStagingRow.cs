namespace ARAS.Main.Oracle.Api.Models.Dtos
{
    public class AdjustmentCreateStagingRow
    {
        public string CustomerTrxId { get; set; }
        public long ReceivableActivityId { get; set; }
        public AdjustmentPostingDto AdjustmentsDetails { get; set; }
	}
}
