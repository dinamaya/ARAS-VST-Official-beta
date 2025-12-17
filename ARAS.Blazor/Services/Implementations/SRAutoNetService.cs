using ARAS.Blazor.Models.DTOs;
using ARAS.Blazor.Services.Interfaces;

namespace ARAS.Blazor.Services.Implementations
{
	public class SRAutoNetService : 
		BaseAdjustmentCommandService<AdjustmentCreateDto, SRAutoNetRowDto, object>,
		ISRAutoNetService
	{
		public SRAutoNetService(IConfigService configService, IBaseAdjustmentService<AdjustmentCreateDto, SRAutoNetRowDto, object> baseAdjustment) :
			base(baseAdjustment, configService.GetSRAutoNetUrl(), "srr", Map)
		{
		}

		private static readonly Func<SRAutoNetRowDto, AdjustmentCreateDto> Map = (row) => new()
		{
			InvoiceAmount = row.InvoiceAmount,
			InvoiceNumber = row.InvoiceNumber,
			CustomerName = row.CustomerName,
			CustomerNumber = row.CustomerNumber,
			ReasonCode = row.ReasonCode,
		};
	}
}
