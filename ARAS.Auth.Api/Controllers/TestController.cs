using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ARAS.Auth.Api.Controllers
{
	[Route("api/test")]
	[ApiController]
	public class TestController : ControllerBase
	{
		[HttpGet]
		public string Get()
		{
			return "Auth API is working";
		}
	}
}
