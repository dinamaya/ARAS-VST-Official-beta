using ARAS.Main.Oracle.Api.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ARAS.Main.Oracle.Api.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class TestController : ControllerBase
	{
		private readonly MainDbContext context;
		public TestController(MainDbContext context)
		{
			this.context = context;
		}

		[HttpGet("acc-eu")]
		public async Task<IActionResult> Get()
		{
			var resut = await context.TestAccEndUser.ToListAsync();
			return Ok(resut);
		}
	}
}
