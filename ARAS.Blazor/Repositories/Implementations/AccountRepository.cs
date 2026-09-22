using ARAS.Blazor.App_Code.Globals;
using ARAS.Blazor.Context;
using ARAS.Blazor.Models.DTOs;
using ARAS.Blazor.Models.Entities;
using ARAS.Blazor.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ARAS.Blazor.Repositories.Implementations
{
	public class AccountRepository : IAccountRepository
	{
		private readonly AuthDbContext _context;
		private readonly UserManager<Account> _userManager;

		public AccountRepository(AuthDbContext context, UserManager<Account> userManager)
		{
			_context = context;
			_userManager = userManager;
		}

		public async Task<IEnumerable<AccountRowDto>> GetAll()
		{
			return await _context.AccountsVs
				.Select(a => new AccountRowDto()
				{
					Id = a.Id,
					FirstName = Utils.Security.DecodeString(a.FirstName),
					LastName = Utils.Security.DecodeString(a.LastName),
					Email = Utils.Security.DecodeString(a.Email),
					CreatedBy = Utils.Security.DecodeString(a.Creator) ?? "System",
					GroupCode = a.GroupCode ?? "",
					AccountRole = a.AccountType,
					IsActive = a.IsActive
				}).Take(1000).ToListAsync();
		}

		public async Task<AccountEditRequestDto> GetById(string id)
		{
			return await _context.AccountsVs
				.Where(a => a.Id == id)
				.Select(a => new AccountEditRequestDto()
				{
					Id = a.Id,
					FirstName = Utils.Security.DecodeString(a.FirstName),
					LastName = Utils.Security.DecodeString(a.LastName),
					GroupCode = Utils.Security.DecodeString(a.GroupCode) ?? "",
					AccountRole = string.IsNullOrEmpty(a.AccountType) ? "Unassigned" : Utils.Security.DecodeString(a.AccountType),
					IsActive = a.IsActive
				}).FirstOrDefaultAsync() ?? throw new InvalidOperationException("Account not found. Please contact the administrator");
		}

		public async Task Update(AccountEditRequestDto request)
		{
			Account account = await _userManager.FindByIdAsync(request.Id) ?? throw new InvalidOperationException("Account not found. Please contact the administrator");
			account.GroupCode = Utils.Security.CleanString(request.GroupCode);
			account.IsActive = request.IsActive;
			account.SecurityHash = Utils.Security.GenerateExtendedGuid("", 2);

			var accountUpdateResult = await _userManager.UpdateAsync(account);
			Guards.ThrowInvalidOperationIf(!accountUpdateResult.Succeeded, "Failed to update account. " + Utils.GetErrorDescription(accountUpdateResult));

		}
	}
}
