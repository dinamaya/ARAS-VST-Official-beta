namespace ARAS.Main.SSMS.Api.App_Code.Globals.Helpers
{
    public static class StagingHeaderIdHelper
    {
        public const long Scale = 1_000_000_000;

        public static long Generate(long requestId, long adjustmentId)
        {
            if (requestId <= 0) throw new ArgumentOutOfRangeException(nameof(requestId));
            if (adjustmentId <= 0 || adjustmentId >= Scale) throw new ArgumentOutOfRangeException(nameof(adjustmentId));
            if (requestId > long.MaxValue / Scale) throw new OverflowException("requestId is too large to generate a staging header id.");

            checked
            {
                return (requestId * Scale) + adjustmentId;
            }
        }

        public static (long RequestId, long AdjustmentId) Parse(long headerId)
        {
            if (headerId <= 0) throw new ArgumentOutOfRangeException(nameof(headerId));

            long requestId = headerId / Scale;
            long adjustmentId = headerId % Scale;

            if (requestId <= 0 || adjustmentId <= 0) throw new ArgumentException("Invalid staging header id format.", nameof(headerId));

            return (requestId, adjustmentId);
        }
    }
}