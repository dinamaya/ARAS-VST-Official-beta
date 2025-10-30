using ARAS.Main.Oracle.Api.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
