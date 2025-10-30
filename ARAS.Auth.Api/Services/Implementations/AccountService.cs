using ARAS.Auth.Api.App_Code.Globals;
using ARAS.Auth.Api.App_Code.Globals.Constants;
using ARAS.Auth.Api.Context;
using ARAS.Auth.Api.Models.Dtos;
using ARAS.Auth.Api.Models.Entities;
using ARAS.Auth.Api.Models.SQLViews;
using ARAS.Auth.Api.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ARAS.Auth.Api.Services.Implementations
{
	public class AccountService : IAccountService
	{
		private readonly AuthDbContext _context;
		private readonly RoleManager<IdentityRole> _roleManager;
		private readonly UserManager<Account> _userManager;

		public AccountService(AuthDbContext context, RoleManager<IdentityRole> roleManager, UserManager<Account> userManager)
		{
			_context = context;
			_roleManager = roleManager;
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

		public async Task<IEnumerable<DropdownOptionDto>> GetAllRoleOptions()
		{
			return (await _roleManager.Roles
				.Select(r => new DropdownOptionDto()
				{
					Value = r.Id,
					Label = r.Name
				})
				.ToListAsync())
				.Prepend(new DropdownOptionDto()
				{
					Value = "",
					Label = "Unassigned"
				});
		}

		public async Task Update(AccountEditRequestDto request)
		{
			Account account = await _userManager.FindByIdAsync(request.Id) ?? throw new InvalidOperationException("Account not found. Please contact the administrator");
			account.FirstName = Utils.Security.CleanString(request.FirstName);
			account.LastName = Utils.Security.CleanString(request.LastName);
			account.GroupCode = Utils.Security.CleanString(request.GroupCode);
			account.IsActive = request.IsActive;

			var accountUpdateResult = await _userManager.UpdateAsync(account);
			if (!accountUpdateResult.Succeeded)
				throw new InvalidOperationException("Failed to update account. " + Utils.GetErrorDescription(accountUpdateResult));

			// === Role handling ===
			// TEST Need thorough testing
			string currentRole = Utils.Security.DecodeString((await _userManager.GetRolesAsync(account)).FirstOrDefault());
			string requestedRole = Utils.Security.CleanString(request.AccountRole);

			if (string.IsNullOrEmpty(requestedRole) || requestedRole.Equals("Unassigned", StringComparison.OrdinalIgnoreCase))
			{
				if (!string.IsNullOrEmpty(currentRole))
				{
					var removeRoleResult = await _userManager.RemoveFromRoleAsync(account, currentRole);
					if (!removeRoleResult.Succeeded)
						throw new InvalidOperationException("Failed to remove role. " + Utils.GetErrorDescription(removeRoleResult));
				}

				return;
			}

			if (string.Equals(currentRole, requestedRole, StringComparison.OrdinalIgnoreCase))
				return;

			if (!string.IsNullOrEmpty(currentRole))
			{
				var removeRoleResult = await _userManager.RemoveFromRoleAsync(account, currentRole);
				if (!removeRoleResult.Succeeded)
					throw new InvalidOperationException("Failed to remove role. " + Utils.GetErrorDescription(removeRoleResult));
			}

			var addRoleResult = await _userManager.AddToRoleAsync(account, requestedRole);
			if (!addRoleResult.Succeeded)
				throw new InvalidOperationException("Failed to add role. " + Utils.GetErrorDescription(addRoleResult));
		}
	}
}
