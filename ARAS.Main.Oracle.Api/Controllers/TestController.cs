using Microsoft.AspNetCore.Mvc;

namespace ARAS.Main.Oracle.Api.Controllers
{
	[Route("api/test")]
	[ApiController]
	public class TestController : ControllerBase
	{
		[HttpGet]
		public string Get()
		{
			return "ARAS Oracle API is working";
		}
	}
}
