using ARAS.Blazor.Models.Complex;
using ARAS.Blazor.Models.Entities;
using ARAS.Blazor.Services.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ARAS.Blazor.Services.Implementations
{
	public class TokenService : ITokenService
	{
        private readonly IConfigService _config;
        private readonly IHttpContextAccessor _contextAccessor;
		private readonly JwtOptions _jwtOptions;

		public TokenService(IConfigService config, IHttpContextAccessor contextAccessor, IOptions<JwtOptions> jwtOptions)
		{
			_config = config;
			_contextAccessor = contextAccessor;
			_jwtOptions = jwtOptions.Value;
		}

		public string? GetToken()
		{
			string? token = null;
			string name = _config.GetTokenName();
			bool? hasToken = _contextAccessor.HttpContext?.Request.Cookies.TryGetValue(name, out token);
			return hasToken is true ? token : null;
		}

		public string GenerateToken(Account account, string role)
		{
			var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_jwtOptions.Key));
			var claims = new Claim[]
			{
				new(ClaimTypes.PrimarySid, account.Id.ToString()),
				new(JwtRegisteredClaimNames.Sub, _jwtOptions.Subject),
				new(JwtRegisteredClaimNames.Jti, account.OpenId + "."+ Guid.NewGuid().ToString() + "." + account.Id.ToString()),
				new(JwtRegisteredClaimNames.Name, account.UserName.ToString()),
				new(JwtRegisteredClaimNames.GivenName, account.FirstName),
				new(JwtRegisteredClaimNames.FamilyName, account.LastName),
				new(ClaimTypes.GroupSid, account.GroupCode ?? ""),
				new(ClaimTypes.Role, role),
				new(ClaimTypes.Hash, account.SecurityHash ?? ""),
			};

			var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			var securityToken = new JwtSecurityToken(
					_jwtOptions.Issuer,
					_jwtOptions.Audience,
					claims,
					expires: DateTime.UtcNow.AddDays(_jwtOptions.DaysDuration),
					signingCredentials: credentials
			);

			return new JwtSecurityTokenHandler().WriteToken(securityToken);
		}
	}
}
