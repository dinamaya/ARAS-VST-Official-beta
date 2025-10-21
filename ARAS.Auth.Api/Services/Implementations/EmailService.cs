using ARAS.Auth.Api.Context;
using ARAS.Auth.Api.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ARAS.Auth.Api.Services.Implementations
{
	public class EmailService : IEmailService
	{
		private readonly AuthDbContext context;

		public EmailService(AuthDbContext context)
		{
			this.context = context;
		}

		public async Task<IEnumerable<string>> GetAll() => await context.EmailAccountsVs.Select(a => a.Email).ToListAsync() ?? [];
		public async Task<IEnumerable<string>> GetApprovers() => await context.EmailAccountsVs.Where(a => a.NormalizedName == "APPROVER").Select(a => a.Email).ToListAsync() ?? [];
		public async Task<IEnumerable<string>> GetValidators() => await context.EmailAccountsVs.Where(a => a.NormalizedName == "VALIDATOR").Select(a => a.Email).ToListAsync() ?? [];
	}
}
