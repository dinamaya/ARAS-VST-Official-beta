using ARAS.Main.SSMS.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.App_Code.Globals;
using ARAS.Main.SSMS.Api.App_Code.Globals.Constants;
using ARAS.Main.SSMS.Api.Context;
using ARAS.Main.SSMS.Api.Repositories.Interfaces;
using ARAS.Main.SSMS.Api.Models.SQLVIews;

namespace ARAS.Main.SSMS.Api.Repositories.Implementations
{
    public class CashDiscountRepository :
        BaseReceiptAdjustmentCommandRepository<BaseAdjustmentCreateDto>,
        ICashDiscountRepository
    {
        public CashDiscountRepository(
            IBaseReceiptAdjustmentRepository<BaseAdjustmentCreateDto> baseAdjustmentRepo, 
            IAdjustmentRepository adjustmentRepo, 
            IInvoiceRepository invoiceRepo) : 
            base("cdr", baseAdjustmentRepo, adjustmentRepo, invoiceRepo)
        {

        }
    }
}
