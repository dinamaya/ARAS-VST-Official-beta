using ARAS.Main.SSMS.Api.App_Code.Globals.Constants;
using ARAS.Main.SSMS.Api.Models.Complex;
using ARAS.Main.SSMS.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.Services.Interfaces;
using Humanizer;
using Microsoft.Extensions.Options;
using System.Net;

namespace ARAS.Main.SSMS.Api.Services.Implementations
{
	public class FileManager : IFileManager
	{
		private readonly IConfigurationService _configService;
		private readonly ILogger<FileManager> _logger;
		private readonly string _storagePath = string.Empty;
		private readonly FileManagerConfig _fileManagerConfig;

		public FileManager(IConfigurationService configService, ILogger<FileManager> logger, IOptions<FileManagerConfig> options)
		{
			_configService = configService;
			_logger = logger;
			_storagePath = _configService.GetAttachmentsDirectory();
			_fileManagerConfig = options.Value;
		}

		public async Task<FileUploadRequirementsDto> GetFileUploadRequirements()
		{
			var allowedTypes = _fileManagerConfig.AllowedFileTypes;
			return await Task.Run(() => new FileUploadRequirementsDto()
			{
				MaxSize = _fileManagerConfig.MaxSize,
				AllowTypesMessage = allowedTypes.Humanize("or"),
				AllowedTypes = allowedTypes
			});
		}

		public string GetAttachmentGroupDirectoryByDate(DateTime date) => Path.Combine(_storagePath, date.ToString(Formats.Date.INPUT));
		public string GetAttachmentGroupDirectoryToday() => GetAttachmentGroupDirectoryByDate(DateTime.Now);
		public async Task UploadAttachmentAsync(IFormFile file, string fileName, DateTime date)
		{
			string directory = GetAttachmentGroupDirectoryByDate(date);

			if (!Directory.Exists(directory))
				Directory.CreateDirectory(directory);

			string filePath = Path.Combine(directory, fileName);

			await UploadAsync(file, filePath);
		}
		private async Task UploadAsync(IFormFile file, string filePath)
		{
			long maxKbSize = _fileManagerConfig.MaxSize * 1024 * 1024;
			if (file.Length > maxKbSize)
				throw new ArgumentOutOfRangeException(nameof(file), $"File size should not exceed {_fileManagerConfig.MaxSize} MB.");

			await using var original = file.OpenReadStream();
			await using var target = new FileStream(filePath, FileMode.Create);

			await original.CopyToAsync(target);
		}
		public string GetUniqueFileName(IFormFile file, string fileName, DateTime date)
		{
			string ext = Path.GetExtension(file.FileName);
			string fn = fileName;

			int suffix = 0;

			while (File.Exists($"{GetAttachmentGroupDirectoryByDate(date)}\\{fn}{ext}"))
			{
				suffix++;
				string currSuffix = "_" + suffix.ToString();
				string preSuffix = "_" + (suffix - 1).ToString();

				fn = fn.EndsWith(preSuffix) ? fn.Replace(preSuffix, currSuffix) : fn + currSuffix;
			}

			return fn + ext;
		}
		public string GetUniqueFileName(string fileName, string formattedName, DateTime date)
		{
			string ext = Path.GetExtension(fileName);
			string fn = formattedName;

			int suffix = 0;

			while (File.Exists($"{GetAttachmentGroupDirectoryByDate(date)}\\{fn}{ext}"))
			{
				suffix++;
				string currSuffix = "_" + suffix.ToString();
				string preSuffix = "_" + (suffix - 1).ToString();

				fn = fn.EndsWith(preSuffix) ? fn.Replace(preSuffix, currSuffix) : fn + currSuffix;
			}

			return fn + ext;
		}
		public async Task<AttachmentDownloadDto> DownloadFile(string fileName)
		{
			string decodedFileName = WebUtility.UrlDecode(fileName);
			string safeFileName = Path.GetFileName(decodedFileName);
			string folder = ExtractDate(fileName);

			string filePath = Path.Combine(_storagePath, folder, safeFileName);
			if (!File.Exists(filePath))
				throw new FileNotFoundException();

			var fileBytes = await File.ReadAllBytesAsync(filePath);

			string contentType = "application/octet-stream";

			return new AttachmentDownloadDto(fileBytes, contentType, filePath);
		}

		private string ExtractDate(string filename) => filename.Split('_')[0];
	}
}
