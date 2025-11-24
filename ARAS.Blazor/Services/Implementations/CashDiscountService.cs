using ARAS.Blazor.Models.DTOs;
using ARAS.Blazor.Services.Interfaces;
using Radzen;

namespace ARAS.Blazor.Services.Implementations
{
	public class CashDiscountService : 
		BaseAdjustmentCommandService<CashDiscountCreateDto, CashDiscountRowDto, CashDiscountCreateValidationDto>, 
		ICashDiscountService
	{
		public CashDiscountService(IConfigService configService, IBaseAdjustmentService<CashDiscountCreateDto, CashDiscountRowDto, CashDiscountCreateValidationDto> baseAdjustment) : 
			base(baseAdjustment, configService.GetCashDiscountsUrl(), "cdr", Map){
		}

		private static readonly Func<CashDiscountRowDto , CashDiscountCreateDto> Map = (row) => new()
		{
			DiscountValue = row.DiscountValue,
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
