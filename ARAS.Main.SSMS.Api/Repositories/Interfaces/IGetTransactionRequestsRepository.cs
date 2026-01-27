using ARAS.Main.SSMS.Api.Models.Dtos;

namespace ARAS.Main.SSMS.Api.Repositories.Interfaces
{
	public interface IGetTransactionRequestsRepository
	{
		Task<IEnumerable<InvoiceAdjustmentRowDto>> GetInvoicedjustmentApprovals(SearchRequestDto data);
		Task<IEnumerable<ReceiptAdjustmentRowDto>> GetReceiptAdjustmentApprovals(SearchRequestDto data);
		Task<IEnumerable<InvoiceAdjustmentRowDto>> GetInvoiceAdjustmentSubmissions(SearchRequestDto data, string role, string fullname);
		Task<IEnumerable<ReceiptAdjustmentRowDto>> GetReceiptAdjustmentSubmissions(SearchRequestDto data, string role, string fullname);
	}
}
