namespace ARAS.Blazor.Models.DTOs
{
    public class APAROffsetAPRowDto
    {
        public string Id { get; set; }
        public double InvoiceAmount { get; set; } = 1_000.00d;
        public string InvoiceNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerNumber { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        public long? CustomerTrxId { get; set; }
        public long? PaymentScheduleId { get; set; }
        public string? CurrencyCode { get; set; }
        public DateTime? GlAccountingDate { get; set; }

        public APAROffsetAPRowDto() { }

        public APAROffsetAPRowDto(InvoiceDetailsDto details)
        {
            Id = details.Id;
            InvoiceAmount = details.InvoiceAmount;
            InvoiceNumber = details.InvoiceNumber;
            InvoiceDate = details.InvoiceDate;
			CustomerName = details.CustomerName;
            CustomerNumber = details.CustomerNumber;
            CustomerTrxId = details.CustomerTrxId;
            PaymentScheduleId = details.PaymentScheduleId;
            CurrencyCode = details.CurrencyCode;
            GlAccountingDate = details.GlAccountingDate;
        }
    }
}

