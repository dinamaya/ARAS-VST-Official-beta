using ARAS.Blazor.Models.DTOs;
using ARAS.Blazor.Services.Interfaces;
using Radzen;

namespace ARAS.Blazor.Services.Implementations
{
	public class BankChargeService : 
		BaseAdjustmentCommandService<AdjustmentCreateDto, BankChargeRowDto, object>,
		IBankChargeService
	{
		public BankChargeService(
			IConfigService configService, 
			IBaseAdjustmentService<AdjustmentCreateDto, BankChargeRowDto, object> baseAdjustmentService) : 
			base(baseAdjustmentService, configService.GetBankChargesUrl(), "bca", Map)
		{

		}

		private static AdjustmentCreateDto Map(BankChargeRowDto row) => new()
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
