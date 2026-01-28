namespace ARAS.Main.SSMS.Api.Models.Dtos
{
    public class ReasonAdjustmentCreateDto
    {
        public ReasonAdjustmentCreateDto(
            long requestId, 
            string reasonCode, 
            double amount, string customerNumber, string customerName)
        {
            RequestId = requestId;
            ReasonCode = reasonCode;
            Amount = amount;
            CustomerNumber = customerNumber;
            CustomerName = customerName;
        }

        public long RequestId { get; set; }
		public string ReasonCode { get; set; }
		public double Amount { get; set; }
		public string CustomerNumber { get; set; }
		public string CustomerName { get; set; }
	}
}
