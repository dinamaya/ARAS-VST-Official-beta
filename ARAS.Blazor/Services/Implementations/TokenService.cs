using ARAS.Blazor.Services.Interfaces;

namespace ARAS.Blazor.Services.Implementations
{
	public class TokenService : ITokenService
	{
        private readonly IConfigService _config;
        private readonly IHttpContextAccessor _contextAccessor;

		public TokenService(IConfigService config, IHttpContextAccessor contextAccessor)
		{
			_config = config;
			_contextAccessor = contextAccessor;
		}

		public string? GetToken()
		{
			string? token = null;
			string name = _config.GetTokenName();
			bool? hasToken = _contextAccessor.HttpContext?.Request.Cookies.TryGetValue(name, out token);
			return hasToken is true ? token : null;
		}
	}
}
