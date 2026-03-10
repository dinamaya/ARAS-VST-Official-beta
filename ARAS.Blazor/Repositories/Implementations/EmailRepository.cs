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
		public async Task<IEnumerable<string>> GetApprovers() => await GetEmailsByRoles("APPROVER", "CNC APPROVER");
		public async Task<IEnumerable<string>> GetValidators() => await GetEmailsByRoles("VALIDATOR", "FSG VALIDATOR");
		public async Task<IEnumerable<string>> GetRequestors() => await context.EmailAccountsVs.AsNoTracking().Where(a => a.NormalizedName == "REQUESTOR").Select(a => a.Email).ToListAsync() ?? [];

		public async Task<IEnumerable<string>> GetNegateRecipients(string role) => role switch
		{
			"Approver" or "CNC Approver" => await GetEmailsByRoles("APPROVER", "CNC APPROVER", "REQUESTOR"),
			"Validator" or "FSG Validator" => await GetEmailsByRoles("VALIDATOR", "FSG VALIDATOR", "REQUESTOR"),
			"FSG Approver" => await GetEmailsByRoles("FSG APPROVER", "REQUESTOR"),
			_ => await GetAll()
		};

		public async Task<IEnumerable<string>> GetUpdateRecipients(string role) => role switch
		{
			"Requestor" => await GetApprovers(),
			"Approver" or "CNC Approver" => await GetValidators(),
			"Validator" or "FSG Validator" => await GetEmailsByRoles("FSG APPROVER"),
			_ => await GetAll()
		};

		private async Task<IEnumerable<string>> GetEmailsByRoles(params string[] roles) =>
			await context.EmailAccountsVs.AsNoTracking()
				.Where(a => roles.Contains(a.NormalizedName))
				.Select(a => a.Email)
				.ToListAsync() ?? [];
	}
}