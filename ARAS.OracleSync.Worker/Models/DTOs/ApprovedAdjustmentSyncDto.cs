namespace ARAS.OracleSync.Worker.Models.DTOs
{
    public record ApprovedAdjustmentSyncDto
    {
        public long RequestId { get; set; }
        public long AdjustmentId { get; set; }
        public long HeaderId { get; set; }
    }
}