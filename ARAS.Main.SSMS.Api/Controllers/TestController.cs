using ARAS.Main.SSMS.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.Repositories.Interfaces;
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
		private readonly IAdjustmentRepository _adjustmentRepo;

		public TestController(IBackgroundJobService bgJobService, IAdjustmentRepository adjustmentRepo)
		{
			_bgJobService = bgJobService;
			_adjustmentRepo = adjustmentRepo;
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

		[HttpGet("ref-number/{groupCode}/{adjustmentTypeCode}")]
		public async Task<string> GetRefNo(string groupCode, string adjustmentTypeCode)
		{
			try
			{
				return await _adjustmentRepo.GenerateReferenceNumber(groupCode, adjustmentTypeCode);
			}
			catch (Exception ex)
			{
				return ex.Message;
			}
		}
	}
}
