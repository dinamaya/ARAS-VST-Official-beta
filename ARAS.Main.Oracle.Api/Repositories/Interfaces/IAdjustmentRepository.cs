using ARAS.Main.Oracle.Api.Models.Dtos;

namespace ARAS.Main.Oracle.Api.Repositories.Interfaces
{
	public interface IAdjustmentRepository
	{
		Task<IEnumerable<string>> GetReasonCodes();
		Task<IEnumerable<ReceivablesActivityDto>> GetReceivableActivities();
		Task Create(AdjustmentPostingDto data);
	}
}
