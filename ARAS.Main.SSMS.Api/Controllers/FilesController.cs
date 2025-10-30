using ARAS.Blazor.App_Code.Globals.Extensions;
using ARAS.Main.SSMS.Api.App_Code.Globals;
using ARAS.Main.SSMS.Api.App_Code.Globals.Constants;
using ARAS.Main.SSMS.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.Services.Interfaces;
using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Net.Mail;
using System.Security.Claims;

namespace ARAS.Main.SSMS.Api.Controllers
{
	[Route("api/files")]
	[ApiController]
	public class FilesController : ControllerBase
	{
		private readonly ILogger<FilesController> _logger;
		private readonly INoteService _attachService;
		private readonly IFileManager _fileManager;

		public FilesController(INoteService attachService, ILogger<FilesController> logger, IFileManager fileManager)
		{
			_attachService = attachService;
			_logger = logger;
			_fileManager = fileManager;
		}

		[HttpGet("requirements")]
		public async Task<ResponseDto<FileUploadRequirementsDto>> GetRequirements()
		{
			var response = new ResponseDto<FileUploadRequirementsDto>();

			try
			{
				response.Result = await _fileManager.GetFileUploadRequirements();
				response.Message = "Requirements Fetched Successfully";
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message);
				return response.Failed(ex.Message);
			}
		}

		[HttpPost("notes/upload/{requestId:long}")]
		public async Task<ResponseDto<string>> CreateNotes(long requestId, [FromForm] IEnumerable<NoteRowDto> notes)
		{
			var response = new ResponseDto<string>();

			try
			{
				NoteCreateDto noteCreate = new(requestId, notes);

				string accountId = User.GetIdentityClaim(ClaimTypes.PrimarySid);
				response.Result = await _attachService.CreateAsync(noteCreate, accountId);
				response.Message = "Notes Created Successfully";
				
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message);
				return response.Failed(ex.Message);
			}
		}

		[HttpGet("notes/{requestId:long}")]
		public async Task<ResponseDto<IEnumerable<NoteRowDto>>> GetNotesByRequestId(long requestId)
		{
			var response = new ResponseDto<IEnumerable<NoteRowDto>>();

			try
			{
				response.Result = await _attachService.GetByRequestId(requestId);
				response.Message = "Notes Fetched Successfully";
				
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message);
				return response.Failed(ex.Message);
			}
		}

		[HttpGet("notes/download/{noteId}")]
		public async Task<IActionResult> Download(string noteId)
		{
			try
			{
				Guards.ThrowInvalidOperationIf(string.IsNullOrWhiteSpace(noteId), "Note ID cannot be null or empty.");

				var note = await _attachService.GetById(noteId);
				var data = await _fileManager.DownloadFile(note.Name);

				return File(data.FileBytes, data.ContentType, data.FilePath);
			}
			catch (FileNotFoundException ex)
			{
				string message = "Attachment not found. Please contact the administrator";
				_logger.LogError(message);
				return NotFound(message);
			}
			catch (DirectoryNotFoundException ex)
			{
				string message = "Directory doesn't exist in the file server. Please contact the administrator";
				_logger.LogError(message);
				return NotFound(message);
			}
			catch (InvalidOperationException ex)
			{
				string message = Exceptions.GetMessage(ex);
				_logger.LogError(message);
				return BadRequest(message);
			}
			catch (Exception ex)
			{
				string message = Exceptions.GetMessage(ex);
				_logger.LogError(message);
				return Problem(message);
			}
		}
	}
}
