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
	}
}
