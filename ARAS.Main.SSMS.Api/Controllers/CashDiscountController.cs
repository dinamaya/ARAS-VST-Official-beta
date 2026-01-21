using ARAS.Blazor.App_Code.Globals.Extensions;
using ARAS.Main.SSMS.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ARAS.Main.SSMS.Api.Controllers
{
	[Route("api/cash-discount")]
	[ApiController]
	public class CashDiscountController : ControllerBase
	{
		private readonly ILogger<CashDiscountController> _logger;
		private readonly ICashDiscountRepository _cashDiscountRepo;
		
		public CashDiscountController(ILogger<CashDiscountController> logger, ICashDiscountRepository cashDiscountRepo)
        {
            _logger = logger;
            _cashDiscountRepo = cashDiscountRepo;
        }

        [HttpGet("test")] public async Task<string> Test() => "Connected to Cash-Discount Endpoint Successfully";

		[HttpPost, Authorize(Roles = "Requestor")]
		public async Task<ResponseDto<long>> Create([FromBody] BaseAdjustmentCreateDto data)
		{
			ResponseDto<long> response = new();
			try
			{
				var requestCreation = new RequestCreationDto<BaseAdjustmentCreateDto>(data, User.GetAccountBasicInfo());

				long requestId = await _cashDiscountRepo.CreateAsync(requestCreation, requestCreation.CreatorId);

				response.Result = requestId;
				response.Message = "Request Created Successfully";
				return response;
			}
			catch (Exception ex)
			{
				return response.Failed(ex.Message);
			}
		}
	}
}
