namespace ARAS.Main.SSMS.Api.Models.Dtos
{
    public class ReceiptAdjustmentUpdateResponseDto
    {
		public double AdjustmentAmount { get; set; }
		public string Remarks { get; set; }
		public string ApiMarker { get; set; }
		public InvoiceDetailsRequestDto Invoice { get; set; }

		public IEnumerable<TransactionHistoryDto> TransactionHistories { get; set; }
		public IEnumerable<NoteRowDto> Notes { get; set; }
	}
}
