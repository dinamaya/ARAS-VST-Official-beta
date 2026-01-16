using ARAS.Blazor.App_Code.Globals.Enums;
using ARAS.Blazor.Models.DTOs;
using ARAS.Blazor.Repositories.Interfaces;
using ARAS.Blazor.Services.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Radzen;

namespace ARAS.Blazor.Services.Implementations
{
	public class CashDiscountService : 
        BaseReceiptAdjustmentCommandService<CashDiscountCreateDto, CashDiscountRowDto>,
        ICashDiscountService
	{
		public CashDiscountService(
			IConfigService configService,
			IBaseReceiptAdjustmentService<CashDiscountCreateDto> baseAdjustmentService) :
			base(baseAdjustmentService, configService.GetCashDiscountsUrl(), "cdr", Map)
		{

		}

		private static readonly Func<CashDiscountRowDto , CashDiscountCreateDto> Map = (row) => new()
		{
			InvoiceAmount = row.InvoiceAmount,
			AdjustmentAmount = row.AdjustmentAmount,
			InvoiceDate = DateTime.Parse(row.InvoiceDate),
			InvoiceNumber = row.InvoiceNumber,
			CustomerName = row.CustomerName,
			CustomerNumber = row.CustomerNumber,
			ReasonCode = row.ReasonCode,
			Remarks = row.Remarks,
		};
    }
}
