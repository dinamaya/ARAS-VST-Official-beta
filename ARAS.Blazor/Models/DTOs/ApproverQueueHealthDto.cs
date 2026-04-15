namespace ARAS.Blazor.Models.DTOs
{
    public class ApproverQueueHealthDto
    {
        public string QueueStatus { get; set; } = string.Empty;
        public int PendingInMyQueue { get; set; }
        public int ApprovedByMe { get; set; }
        public int DeclinedByMe { get; set; }
        public int RejectedByMe { get; set; }
        public int OverdueInMyQueue { get; set; }
        public int ErpPostingCount { get; set; }
        public int LookbackDays { get; set; } = 30;
        public int SlaDays { get; set; } = 2;
    }
}
