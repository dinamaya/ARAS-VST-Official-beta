namespace ARAS.Main.SSMS.Api.Models.Dtos
{
    public class ReceiptAdjustmentUpdateRequestDto
    {
        public string Remarks { get; set; }
        public double AdjustmentAmount { get; set; }

        // Apply Receipt (to Invoice) changes on a request that carries them.
        // Null means "leave the request's invoice applications unchanged".
        public bool? ApplyReceiptEnabled { get; set; }
        public IList<ReceiptInvoiceApplicationSelectionDto>? ReceiptApplications { get; set; }
	}
}
