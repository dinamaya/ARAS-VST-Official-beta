using ARAS.Auth.Api.Context;
using ARAS.Auth.Api.Models.Dtos;
using ARAS.Auth.Api.Models.Entities;
using ARAS.Auth.Api.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ARAS.Auth.Api.Services.Implementations
{
	public class AuthService : IAuthService
	{
		private readonly ITokenService _tokenService;
		private readonly UserManager<Account> _userManager;
		private readonly AuthDbContext _context;

		public AuthService(ITokenService tokenService, UserManager<Account> userManager, AuthDbContext context)
		{
			_tokenService = tokenService;
			_userManager = userManager;
			_context = context;
		}

		public async Task<string> SignInAsync(AccountSignInRequestDto dto)
		{
			if (string.IsNullOrEmpty(dto.Email))
				throw new InvalidOperationException("Email is empty. Please contact the administrator");

			var account = await _context.Accounts.Where(a => a.OpenId == dto.OpenId).FirstOrDefaultAsync();

			if (account == null)
			{
				Account other = new Account();
				var date = DateTime.Now;

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
				if (!createResult.Succeeded)
					throw new InvalidOperationException($"Failed to create account {account.UserName}.\n{string.Join(", ", createResult.Errors.Select(e => e.Description))}");

				var setEmailResult = await _userManager.SetEmailAsync(other, dto.Email);
				if (!setEmailResult.Succeeded) 
					throw new InvalidOperationException($"Failed to create account {account.UserName}.\n{string.Join(", ", setEmailResult.Errors.Select(e => e.Description))}");
				
				var setUsernameResult = await _userManager.SetEmailAsync(other, dto.Email);
				if (!setUsernameResult.Succeeded) 
					throw new InvalidOperationException($"Failed to create account {account.UserName}.\n{string.Join(", ", setUsernameResult.Errors.Select(e => e.Description))}");
			}

			var role = (await _userManager.GetRolesAsync(account)).FirstOrDefault() ?? throw new UnauthorizedAccessException("Account is not authorize");
			
			return _tokenService.GenerateToken(account, role);
		}
	}
}
