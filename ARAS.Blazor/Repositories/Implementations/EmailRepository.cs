using ARAS.Blazor.Context;
using ARAS.Blazor.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ARAS.Blazor.Repositories.Implementations
{
	public class EmailRepository : IEmailRepository
	{
		private readonly AuthDbContext context;
		public EmailRepository(AuthDbContext context) => this.context = context;

		public async Task<IEnumerable<string>> GetAll() => await context.EmailAccountsVs.AsNoTracking().Select(a => a.Email).ToListAsync() ?? [];
		public async Task<IEnumerable<string>> GetApprovers() => await context.EmailAccountsVs.AsNoTracking().Where(a => a.NormalizedName == "APPROVER").Select(a => a.Email).ToListAsync() ?? [];
		public async Task<IEnumerable<string>> GetValidators() => await context.EmailAccountsVs.AsNoTracking().Where(a => a.NormalizedName == "VALIDATOR").Select(a => a.Email).ToListAsync() ?? [];
		public async Task<IEnumerable<string>> GetRequestors() => await context.EmailAccountsVs.AsNoTracking().Where(a => a.NormalizedName == "REQUESTOR").Select(a => a.Email).ToListAsync() ?? [];

		public async Task<IEnumerable<string>> GetNegateRecipients(string role) => role switch
		{
			"Approver" => await context.EmailAccountsVs.AsNoTracking().Where(a => a.NormalizedName == "APPROVER" || a.NormalizedName == "REQUESTOR").Select(a => a.Email).ToListAsync() ?? [],
			"Validator" => await context.EmailAccountsVs.AsNoTracking().Where(a => a.NormalizedName == "VALIDATOR" || a.NormalizedName == "REQUESTOR").Select(a => a.Email).ToListAsync() ?? [],
			_ => await GetAll()
		};

		public async Task<IEnumerable<string>> GetUpdateRecipients(string role) => role switch
		{
			"Requestor" => await GetApprovers(),
			"Approver" => await GetValidators(),
			_ => await GetAll()
		};
	}
}
