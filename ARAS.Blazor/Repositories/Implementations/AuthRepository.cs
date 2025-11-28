using ARAS.Blazor.App_Code.Globals;
using ARAS.Blazor.App_Code.Globals.Constants;
using ARAS.Blazor.Context;
using ARAS.Blazor.Models.DTOs;
using ARAS.Blazor.Models.Entities;
using ARAS.Blazor.Repositories.Interfaces;
using ARAS.Blazor.Services.Implementations;
using ARAS.Blazor.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ARAS.Blazor.Repositories.Implementations
{
	public class AuthRepository : IAuthRepository
	{
		private readonly ITokenService _tokenService;
		private readonly UserManager<Account> _userManager;
		private readonly AuthDbContext _context;

		public AuthRepository(ITokenService tokenService, UserManager<Account> userManager, AuthDbContext context)
		{
			_tokenService = tokenService;
			_userManager = userManager;
			_context = context;
		}

		public async Task<string> GetSecurityHashByOId(string oid)
		{
			return await _context.Accounts
				.Where(a => a.OpenId == oid)
				.Select(a => a.SecurityHash ?? string.Empty)
				.FirstOrDefaultAsync() ?? string.Empty;
		}

		public async Task<string> SignInAsync(AccountSignInRequestDto dto)
		{
			Guards.ThrowInvalidOperationIf(string.IsNullOrEmpty(dto.Email), "Email is empty. Please contact the administrator");

			var account = await _context.Accounts.Where(a => a.OpenId == dto.OpenId).FirstOrDefaultAsync();

			if (account == null)
			{
				Account other = new Account();
				var date = DateTime.UtcNow.ToLocalTime();

				other.Email = dto.Email;
				other.UserName = dto.Email;
				other.OpenId = dto.OpenId;
				other.FirstName = dto.FirstName;
				other.LastName = dto.LastName;
				other.GroupCode = "";

				other.CreatedBy = "ARAS-AUTH-API";
				other.DateCreated = date;
				other.ModifiedBy = "ARAS-AUTH-API";
				other.DateModified = date;
				other.IsActive = true;

				var createResult = await _userManager.CreateAsync(other);
				Guards.ThrowInvalidOperationIf(!createResult.Succeeded, $"Failed to create account {other.UserName}.\n{string.Join(", ", createResult.Errors.Select(e => e.Description))}");

				var setEmailResult = await _userManager.SetEmailAsync(other, dto.Email);
				Guards.ThrowInvalidOperationIf(!setEmailResult.Succeeded, $"Failed to create account {other.UserName}.\n{string.Join(", ", setEmailResult.Errors.Select(e => e.Description))}");

				var setUsernameResult = await _userManager.SetUserNameAsync(other, dto.Email);
				Guards.ThrowInvalidOperationIf(!setUsernameResult.Succeeded, $"Failed to create account {other.UserName}.\n{string.Join(", ", setUsernameResult.Errors.Select(e => e.Description))}");
			}

			Guards.ThrowInvalidOperationIf(account != null && !account.IsActive, "Account is deactivated. Please contact the administrator");

			var role = (await _userManager.GetRolesAsync(account)).FirstOrDefault() ?? throw new UnauthorizedAccessException("Account is not authorize");

			return _tokenService.GenerateToken(account, role);
		}
	}
}
