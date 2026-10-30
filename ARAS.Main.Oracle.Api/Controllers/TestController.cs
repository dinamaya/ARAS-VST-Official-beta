using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ARAS.Main.Oracle.Api.Controllers
{
	[Route("api/test")]
	[ApiController]
	[Authorize]
	public class TestController : ControllerBase
	{
		// Plain health check for the gateway / monitoring; returns no data.
		[HttpGet, AllowAnonymous]
		public string Get()
		{
			return "ARAS Oracle API is working";
		}
	}
}
