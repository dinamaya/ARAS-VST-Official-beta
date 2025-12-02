using ARAS.Blazor.Models.DTOs;
using ARAS.Blazor.Repositories.Interfaces;
using ARAS.Blazor.Services.Interfaces;

namespace ARAS.Blazor.Services.Implementations
{
	public class WriteOffService :
		BaseAdjustmentCommandService<AdjustmentCreateDto, SmallAmountRowDto, object>,
		IWriteOffService
	{
		public WriteOffService(
			IConfigService configService,
			IBaseAdjustmentService<AdjustmentCreateDto, SmallAmountRowDto, object> baseAdjustmentService) :
			base(baseAdjustmentService, configService.GetBankChargesUrl(), "bca", Map)
		{

		}

		private static AdjustmentCreateDto Map(SmallAmountRowDto row) => new()
		{
			AdjustmentAmount = row.AdjustmentAmount,
			InvoiceAmount = row.InvoiceAmount,
			InvoiceDate = DateTime.Parse(row.InvoiceDate),
			InvoiceNumber = row.InvoiceNumber,
			CustomerName = row.CustomerName,
			CustomerNumber = row.CustomerNumber,
			ReasonCode = row.ReasonCode,
			Remarks = row.Remarks,
		};
	}
}
