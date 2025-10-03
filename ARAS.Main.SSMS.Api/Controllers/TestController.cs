using ARAS.Main.SSMS.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ARAS.Main.SSMS.Api.Controllers
{
	[Route("api/test")]
	[ApiController]
	public class TestController : ControllerBase
	{
		private readonly IBackgroundJobService _bgJobService;

		public TestController(IBackgroundJobService bgJobService)
		{
			_bgJobService = bgJobService;
		}

		[HttpGet("email")]
		public async Task SendTestEmail()
		{
			try
			{
				await _bgJobService.RunTestJob();
			}
			catch (Exception ex)
			{
			}
		}
	}
}
