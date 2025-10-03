using ARAS.Auth.Api.App_Code.Globals.Constants;
using ARAS.Auth.Api.Models.Complex;
using ARAS.Auth.Api.Models.Entities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ARAS.Auth.Api.Services.Interfaces
{
	public interface ITokenService
	{
		public string GenerateToken(Account account, string role);
	}
}
