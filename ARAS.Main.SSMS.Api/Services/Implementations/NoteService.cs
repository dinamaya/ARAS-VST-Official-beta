using ARAS.Main.SSMS.Api.App_Code.Globals;
using ARAS.Main.SSMS.Api.App_Code.Globals.Constants;
using ARAS.Main.SSMS.Api.Context;
using ARAS.Main.SSMS.Api.Models.Complex;
using ARAS.Main.SSMS.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.Models.Entities;
using ARAS.Main.SSMS.Api.Services.Interfaces;
using Humanizer;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ARAS.Main.SSMS.Api.Services.Implementations
{
	public class NoteService : INoteService
	{
		private readonly ILogger<NoteService> _logger;
		private readonly IFileManager _fileManager;
		private readonly MainDbContext _context;

		public NoteService(ILogger<NoteService> logger, IFileManager fileManager, MainDbContext context)
		{
			_logger = logger;
			_fileManager = fileManager;
			_context = context;
		}

		public async Task<string> CreateAsync(NoteCreateDto data, string createdBy)
		{
			foreach (var note in data.Notes)
			{
				var date = DateTime.Now;
				string formattedName = $"{DateTime.Today.ToString("MMddyyyy")}_{Utils.Security.GenerateExtendedGuid(string.Empty, 2)}";
				var _note = new Note();
				_note.AttachmentName = string.Empty;

				if (note.AttachmentData != null)
				{
					_note.AttachmentName = _fileManager.GetUniqueFileName(note.AttachmentName, formattedName, date);
					await _fileManager.UploadAttachmentAsync(note.AttachmentData, _note.AttachmentName, date);
				}

				_note.RequestId = data.RequestId;
				_note.Remarks = note.Remarks ?? "";

				_note.CreatedBy = createdBy;
				_note.DateCreated = date;
				_note.ModifiedBy = createdBy;
				_note.DateModified = date;
				_note.IsActive = true;

				await _context.Notes.AddAsync(_note);
				await _context.SaveChangesAsync();
			}

			return string.Empty;
		}

		public async Task<IEnumerable<NoteRowDto>> GetByRequestId(long requestId)
		{
			return await _context.VwNotes
				.Where(n => n.RequestId == requestId)
				.OrderBy(n => n.DateCreated)
				.Select(n => new NoteRowDto()
				{
					Id = n.Id,
					Remarks = n.Remarks,
					AttachmentData = null,
					AttachmentName = n.AttachmentName,
					DateUploaded = n.DateCreated.ToString(Formats.Date.DISPLAY_COMPLETE),
					Uploader = n.UploaderLastName + ", " + n.UploaderFirstName,
				})
				.ToListAsync();
		}

		public async Task<AttachmentDto> GetById(string Id)
		{
			return await _context.Notes
				.Where(n => n.Id == Id)
				.Select(n => new AttachmentDto()
				{
					Name = n.AttachmentName,
					DateCreated = n.DateCreated,
				})
				.FirstOrDefaultAsync() ?? throw new InvalidOperationException();
		}

	}
}
