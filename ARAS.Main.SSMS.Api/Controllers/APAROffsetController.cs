using ARAS.Blazor.App_Code.Globals.Extensions;
using ARAS.Main.SSMS.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ARAS.Main.SSMS.Api.Controllers
{
    [Route("api/apar-offset")]
    [ApiController]
    public class APAROffsetController : ControllerBase
    {
        private readonly ILogger<APAROffsetController> _logger;
        private readonly IAPAROffsetRepository _aparOffsetRepo;

        public APAROffsetController(ILogger<APAROffsetController> logger, IAPAROffsetRepository aparOffsetRepo)
        {
            _logger = logger;
            _aparOffsetRepo = aparOffsetRepo;
        }

        [HttpPost]
        public async Task<ResponseDto<long>> Create([FromBody] AdjustmentRequestCreationDto<APAROffsetCreateDto> data)
        {
            ResponseDto<long> response = new();
            try
            {
                var accountInfo = User.GetAccountBasicInfo();

                var requestCreation = new RequestCreationDto<AdjustmentRequestCreationDto<APAROffsetCreateDto>>(data, accountInfo.GroupCode, accountInfo.FullName);

                long requestId = await _aparOffsetRepo.CreateAsync(requestCreation, accountInfo.Id);

                response.Result = requestId;
                response.Message = "Request Created Successfully";
                return response;
            }
            catch (Exception ex)
            {
                return response.Failed(ex.Message);
            }
        }

		[HttpPost("update/{requestId:long}"), Authorize(Roles = "Requestor")]
		public async Task<ResponseDto<string>> Update(long requestId, [FromBody] AdjustmentRequestCreationDto<APAROffsetCreateDto> data)
		{
			ResponseDto<string> response = new ResponseDto<string>();
			try
			{
				var accountInfo = User.GetAccountBasicInfo();
				var requestCreation = new RequestCreationDto<AdjustmentRequestCreationDto<APAROffsetCreateDto>>(data, accountInfo.GroupCode, accountInfo.FullName);

				await _aparOffsetRepo.UpdateAsync(requestId, requestCreation, accountInfo.Id);

				response.Result = "Success";
				response.Message = "Request Updated Successfully";
				return response;
			}
			catch (Exception ex)
			{
				return response.Failed(ex.Message);
			}
		}
	}
}
