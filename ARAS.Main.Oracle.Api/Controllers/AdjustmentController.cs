using ARAS.Main.Oracle.Api.App_Code.Globals.Constants;
using ARAS.Main.Oracle.Api.Models.Dtos;
using ARAS.Main.Oracle.Api.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;

namespace ARAS.Main.Oracle.Api.Controllers
{
	[Route("api/adjustment")]
	[ApiController]
	public class AdjustmentController : ControllerBase
	{
		private readonly ILogger<InvoiceController> _logger;
		private readonly IAdjustmentRepository _adjustRepo;

		public AdjustmentController(ILogger<InvoiceController> logger, IAdjustmentRepository adjustRepo)
		{
			_logger = logger;
			_adjustRepo = adjustRepo;
		}

		[HttpGet("receivable-activities")]
		public async Task<ResponseDto<IEnumerable<ReceivablesActivityDto>>> GetReceivableActivities()
		{
			ResponseDto<IEnumerable<ReceivablesActivityDto>> _response = new();
			try
			{
				_response.Result = await _adjustRepo.GetReceivableActivities();

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

		[HttpPost]
		public async Task<ResponseDto<string>> Create([FromBody] AdjustmentPostingDto data)
		{
			ResponseDto<string> _response = new();
			try
			{
				await _adjustRepo.Create(data);
				_response.Result = "Success";
				_response.Message = "Adjustment created successfully.";
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
	}
}
