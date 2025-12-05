using ARAS.Blazor.App_Code.Globals.Extensions;
using ARAS.Main.SSMS.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ARAS.Main.SSMS.Api.Controllers
{
    [Route("api/ar-invoice-offsetting")]
    [ApiController]
    public class ARInvoiceOffsettingController : ControllerBase
    {
        private readonly ILogger<ARInvoiceOffsettingController> _logger;
        private readonly IARInvoiceOffsettingRepository _arInvoiceOffsettingRepo;
        private readonly IRequestRepository _requestRepo;

        public ARInvoiceOffsettingController(ILogger<ARInvoiceOffsettingController> logger, IARInvoiceOffsettingRepository arInvoiceOffsettingRepo, IRequestRepository requestRepo)
        {
            _logger = logger;
            _arInvoiceOffsettingRepo = arInvoiceOffsettingRepo;
            _requestRepo = requestRepo;
        }

        [HttpPost, Authorize(Roles = "Requestor")]
        public async Task<ResponseDto<long>> Create([FromBody] AdjustmentRequestCreationDto<ARInvoiceOffsettingCreateDto> data)
        {
            ResponseDto<long> response = new ();
            try
            {
                var accountInfo = User.GetAccountBasicInfo();
                var requestCreation = new RequestCreationDto<AdjustmentRequestCreationDto<ARInvoiceOffsettingCreateDto>>(data, accountInfo.GroupCode, accountInfo.FullName);
                long requestId = await _arInvoiceOffsettingRepo.CreateAsync(requestCreation, accountInfo.Id);
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
        public async Task<ResponseDto<string>> Update(long requestId, [FromBody] AdjustmentRequestCreationDto<ARInvoiceOffsettingCreateDto> data)
        {
            ResponseDto<string> response = new ResponseDto<string>();
            try
            {
                var accountInfo = User.GetAccountBasicInfo();
                var requestCreation = new RequestCreationDto<AdjustmentRequestCreationDto<ARInvoiceOffsettingCreateDto>>(data, accountInfo.GroupCode, accountInfo.FullName);
                await _arInvoiceOffsettingRepo.UpdateAsync(requestId, requestCreation, accountInfo.Id);
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
