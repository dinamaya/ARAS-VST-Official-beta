using ARAS.Main.Oracle.Api.App_Code.Globals.Constants;
using ARAS.Main.Oracle.Api.Models.Dtos;
using ARAS.Main.Oracle.Api.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;

namespace ARAS.Main.Oracle.Api.Controllers
{
	[Route("api/invoice")]
	[ApiController]
	public class InvoiceController : ControllerBase
	{
		private readonly ILogger<InvoiceController> _logger;
		private readonly IInvoiceRepository _invoiceRepo;

		public InvoiceController(ILogger<InvoiceController> logger, IInvoiceRepository invoiceRepo)
		{
			_logger = logger;
			_invoiceRepo = invoiceRepo;
		}

		[HttpGet("no/{invoiceNo}")]
		public async Task<ResponseDto<InvoiceDetailsDto>> GetInvoiceNumbers(string invoiceNo)
		{
			ResponseDto<InvoiceDetailsDto> _response = new();
			try
			{
				_response.Result = await _invoiceRepo.GetInvoiceNo(invoiceNo);

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

        [HttpGet("ap/no/{invoiceNo}")]
        public async Task<ResponseDto<InvoiceAPDetailsDto>> GetAPInvoice(string invoiceNo)
        {
            ResponseDto<InvoiceAPDetailsDto> _response = new();
            try
            {
                _response.Result = await _invoiceRepo.GetAPInvoiceNo(invoiceNo);
                return _response;
            }
            catch (OracleException)
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

		[HttpGet("details/{invoiceNo}")]
		public async Task<ResponseDto<IEnumerable<InvoiceDetailsDto>>> GetInvoiceDetails(string invoiceNo)
		{
			ResponseDto<IEnumerable<InvoiceDetailsDto>> _response = new();
			try
			{
				_response.Result = await _invoiceRepo.GetInvoiceDetails(invoiceNo);

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
