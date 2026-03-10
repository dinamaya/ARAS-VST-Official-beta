using ARAS.Main.SSMS.Api.Context;
using ARAS.Main.SSMS.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ARAS.Main.SSMS.Api.Repositories.Implementations
{
	public class StatusRepository : IStatusRepository
	{
		private readonly MainDbContext _context;

		public StatusRepository(MainDbContext context)
		{
			_context = context;
		}

		public async Task<string> GetIdByName(string name) => 
			await _context.Statuses
			.AsNoTracking()
			.Where(s => s.Name == NormalizeStatusName(name))
			.Select(s => s.Id)
			.FirstOrDefaultAsync();

		private static string NormalizeStatusName(string? name) => name switch
		{
			"Pending" => "For CNC Approval",
			"Resubmitted" => "For CNC Approval",
			"Approved" => "For ERP Posting",
			_ => name ?? string.Empty
		};
	}
}