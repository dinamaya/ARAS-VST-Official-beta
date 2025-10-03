using ARAS.Auth.Api.Models.Complex;
using ARAS.Auth.Api.Models.Entities;
using ARAS.Auth.Api.Services.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ARAS.Auth.Api.Services.Implementations
{
	public class TokenService : ITokenService
	{
		private readonly JwtOptions _jwtOptions;
		public TokenService(IOptions<JwtOptions> jwtOptions)
		{
			_jwtOptions = jwtOptions.Value;
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
				new(ClaimTypes.Role, role),
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
