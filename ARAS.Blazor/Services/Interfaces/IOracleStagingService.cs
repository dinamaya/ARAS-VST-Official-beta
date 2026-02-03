using ARAS.Main.Oracle.Api.Models.Dtos;

namespace ARAS.Blazor.Services.Interfaces
{
	public interface IOracleStagingService
	{
		Task Create(IEnumerable<AdjustmentPostingDto> data);
	}
}
