using ARAS.Main.Oracle.Api.App_Code.Globals.Constants;
using ARAS.Main.Oracle.Api.Models.Dtos;
using ARAS.Main.Oracle.Api.Models.Entities;
using ARAS.Main.Oracle.Api.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;

namespace ARAS.Main.Oracle.Api.Controllers
{
	[Route("api/adjustment")]
	[ApiController]
	[Authorize]
	public class AdjustmentController : ControllerBase
	{
		private readonly ILogger<InvoiceController> _logger;
		private readonly IAdjustmentRepository _adjustRepo;

		public AdjustmentController(ILogger<InvoiceController> logger, IAdjustmentRepository adjustRepo)
		{
			_logger = logger;
			_adjustRepo = adjustRepo;
		}

		[HttpGet("reason-codes")]
		public async Task<ResponseDto<IEnumerable<string>>> GetReasonCodes()
		{
			ResponseDto<IEnumerable<string>> _response = new();
			try
			{
				_response.Result = await _adjustRepo.GetReasonCodes();

				return _response;
			}
			catch (OracleException ex)
			{
				_logger.LogError(Exceptions.CANT_CONNECT);
				return _response.Failed(Exceptions.CANT_CONNECT);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message);
				return _response.Failed(ex.Message);
			}
		}

		// Posts to Oracle Fusion. Only the final approver (FSG Approver) posts.
		[HttpPost("stage"), Authorize(Roles = "FSG Approver")]
		public async Task<ResponseDto<string>> CreateStageRow([FromBody] IEnumerable<AdjustmentPostingDto> data)
		{
			ResponseDto<string> _response = new();
			try
			{
				await _adjustRepo.Create(data);
				_response.Result = "Success";
				_response.Message = "Adjustment created successfully.";
				return _response;
			}
			catch (DbUpdateException ex)
			{
				_logger.LogError(ex, "Oracle staging failed with a database update error. {ErrorMessage}", ex.InnerException?.Message ?? ex.Message);
				return _response.Failed(Exceptions.ADJUSTMENT_POSTED);
			}
			catch (OracleException ex)
			{
				_logger.LogError(ex, "Oracle staging failed with an Oracle exception. {ErrorMessage}", ex.Message);
				return _response.Failed(Exceptions.CANT_CONNECT);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Oracle staging failed with an unexpected exception. {ErrorMessage}", ex.Message);
				return _response.Failed(ex.Message);
			}
		}
	}
}
