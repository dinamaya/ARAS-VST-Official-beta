using ARAS.Blazor.Models.DTOs;
using ARAS.Blazor.Services.Interfaces;

namespace ARAS.Blazor.Services.Implementations
{
	public class SRAutoNetService : 
		BaseAdjustmentCommandService<SRAutoNetCreateDto, SRAutoNetRowDto, object>,
		ISRAutoNetService
	{
		public SRAutoNetService(IConfigService configService, IBaseAdjustmentService<SRAutoNetCreateDto, SRAutoNetRowDto, object> baseAdjustment) :
			base(baseAdjustment, configService.GetSRAutoNetUrl(), "srr", Map)
		{
		}

		private static readonly Func<SRAutoNetRowDto, SRAutoNetCreateDto> Map = (row) => new()
		{
			InvoiceAmount = row.InvoiceAmount,
			InvoiceDate = DateTime.Parse(row.InvoiceDate),
			InvoiceNumber = row.InvoiceNumber,
			CustomerName = row.CustomerName,
			CustomerNumber = row.CustomerNumber,
			ReasonCode = row.ReasonCode,
			Remarks = row.Remarks
		};
	}
}
