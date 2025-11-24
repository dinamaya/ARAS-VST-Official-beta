using ARAS.Blazor.Context;
using ARAS.Blazor.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ARAS.Blazor.Repositories.Implementations
{
	public class EmailRepository : IEmailRepository
	{
		private readonly AuthDbContext context;
		public EmailRepository(AuthDbContext context) => this.context = context;

		public async Task<IEnumerable<string>> GetAll() => await context.EmailAccountsVs.Select(a => a.Email).ToListAsync() ?? [];
		public async Task<IEnumerable<string>> GetApprovers() => await context.EmailAccountsVs.Where(a => a.NormalizedName == "APPROVER").Select(a => a.Email).ToListAsync() ?? [];
		public async Task<IEnumerable<string>> GetValidators() => await context.EmailAccountsVs.Where(a => a.NormalizedName == "VALIDATOR").Select(a => a.Email).ToListAsync() ?? [];
	}
}
