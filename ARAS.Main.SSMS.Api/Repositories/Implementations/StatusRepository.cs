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

		public async Task<string> GetIdByName(string name)
		{
			return await _context.Statuses.Where(s => s.Name == name).Select(s => s.Id).FirstOrDefaultAsync();
		}
	}
}
