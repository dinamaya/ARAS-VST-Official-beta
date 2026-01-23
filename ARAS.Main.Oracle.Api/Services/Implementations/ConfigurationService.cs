using ARAS.Main.Oracle.Api.Models.Dtos;
using ARAS.Main.Oracle.Api.Services.Interfaces;

namespace ARAS.Main.Oracle.Api.Services.Implementations
{
	public class ConfigurationService(IConfiguration config) : IConfigurationService
	{
		public IEnumerable<string> GetReasonCodes() => config.GetSection("DropdownOptions:ReasonCodes").Get<IEnumerable<string>>();
		public bool IsOntest() => config.GetValue<bool>("TestConfig:OnTest");
	}
}
