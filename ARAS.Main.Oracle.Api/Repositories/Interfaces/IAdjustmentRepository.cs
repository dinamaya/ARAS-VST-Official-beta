using ARAS.Main.Oracle.Api.Models.Dtos;
using ARAS.Main.Oracle.Api.Models.Entities;
using Oracle.ManagedDataAccess.Client;

namespace ARAS.Main.Oracle.Api.Repositories.Interfaces
{
	public interface IAdjustmentRepository
	{
		Task<IEnumerable<string>> GetReasonCodes();
		Task<IEnumerable<ReceivablesActivityDto>> GetReceivableActivities();
		Task<ReceivablesActivityDto> GetReceivableActivityByName(string adjustmentActivity);
		Task<ReceivablesActivityDto> GetReceivableActivityByName(OracleConnection oracleConnection, string adjustmentActivity);
		Task Create(IEnumerable<AdjustmentPostingDto> data);
		Task<IEnumerable<ARAdjustmentsStaging>> GetAll();
		Task<IEnumerable<PostedResponseDto>> GetPosted(IEnumerable<long> adjustmentIds);
	}
}
