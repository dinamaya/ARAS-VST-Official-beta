using ARAS.Main.Oracle.Api.App_Code.Globals.Constants;
using ARAS.Main.Oracle.Api.Models.Dtos;
using ARAS.Main.Oracle.Api.Repositories.Interfaces;
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

		[HttpGet("/api/single/invoice/details")]
		public async Task<ResponseDto<InvoiceDetailsDto>> GetOneInvoiceDetails([FromQuery] InvoiceDetailsRequestDto data)
		{
			ResponseDto<InvoiceDetailsDto> _response = new();
			try
			{
				_response.Result = await _invoiceRepo.GetOneInvoiceDetails(data);
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

		[HttpGet]
		public async Task<ResponseDto<IEnumerable<InvoiceDetailsDto>>> GetInvoiceDetails([FromQuery] SearchRequestDto searchRequest)
		{
			ResponseDto<IEnumerable<InvoiceDetailsDto>> _response = new();
			try
			{
				_response.Result = await _invoiceRepo.GetInvoiceDetails(searchRequest);
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

		[HttpGet("ap")]
		public async Task<ResponseDto<IEnumerable<InvoiceDetailsDto>>> GetAPInvoiceDetails([FromQuery] SearchRequestDto searchRequest)
		{
			ResponseDto<IEnumerable<InvoiceDetailsDto>> _response = new();
			try
			{
				_response.Result = await _invoiceRepo.GetAPInvoiceDetails(searchRequest);
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
		public async Task<ResponseDto<InvoiceAPDetailsDto>> GetAPInvoiceNo(string invoiceNo)
		{
			ResponseDto<InvoiceAPDetailsDto> _response = new();
			try
			{
				_response.Result = await _invoiceRepo.GetAPInvoiceNo(invoiceNo);
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

		[HttpGet("cn/{invoiceNo}")]
		public async Task<ResponseDto<IEnumerable<InvoiceDetailsDto>>> GetInvoiceCNDetails(string invoiceNo)
		{
			ResponseDto<IEnumerable<InvoiceDetailsDto>> _response = new();
			try
			{
				_response.Result = await _invoiceRepo.GetCnInvoiceDetails(invoiceNo);
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

		[HttpGet("sr/{invoiceNo}")]
		public async Task<ResponseDto<IEnumerable<SearchCNDetailsRowDto>>> GetSRAutoNetCNDetails(string invoiceNo)
		{
			ResponseDto<IEnumerable<SearchCNDetailsRowDto>> _response = new();
			try
			{
				_response.Result = await _invoiceRepo.GetSRAutoNetCNDetails(invoiceNo);
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

		[HttpGet("customer-trx-id")]
		public async Task<ResponseDto<string>> GetCustomerTrxId([FromQuery] CustomerInvoiceRequestDto data)
		{
			ResponseDto<string> _response = new();
			try
			{
				_response.Result = await _invoiceRepo.GetCustomerTrxIdByInvoiceDetails(data);
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
