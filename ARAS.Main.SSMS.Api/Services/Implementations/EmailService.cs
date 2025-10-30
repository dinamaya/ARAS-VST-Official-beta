using ARAS.Main.SSMS.Api.Models.Complex;
using ARAS.Main.SSMS.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.Services.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Scriban;
using Scriban.Syntax;
using System;

namespace ARAS.Main.SSMS.Api.Services.Implementations
{
	public class EmailService : IEmailService
	{

		private readonly string _testTemplate = Path.Combine("App_Code", "Scriban", "ToApproverTemplate.sbn");
		private readonly string _requestPendingTemplate = Path.Combine("App_Code", "Scriban", "RequestPending.html");
		private readonly string _requestApprovedTemplate = Path.Combine("App_Code", "Scriban", "RequestApproved.html");
		private readonly string _requestValidatedTemplate = Path.Combine("App_Code", "Scriban", "RequestValidated.html");
		private readonly string _requestDeclinedTemplate = Path.Combine("App_Code", "Scriban", "RequestDeclined.html");
		private readonly string _requestRejectedTemplate = Path.Combine("App_Code", "Scriban", "RequestRejected.html");
		private readonly string _requestUpdatedTemplate = Path.Combine("App_Code", "Scriban", "RequestUpdated.html");

		private readonly EmailServiceConfig _emailServiceConfig;
		private readonly IConfigurationService _configService;
		private readonly ILogger<IEmailService> _logger;

		public EmailService(IOptions<EmailServiceConfig> emailServiceOption, IConfigurationService configService, ILogger<IEmailService> logger)
		{
			_emailServiceConfig = emailServiceOption.Value;
			_configService = configService;
			_logger = logger;
		}

		public async Task<TaskResultDto> SendRequestPending(ProceedEmailDto model)
		{
			try
			{
				var emailModel = new EmailRequestDto<ProceedEmailDto>(model, _configService.GetFrontendBaseUrl($"approvals/cash-discount/{model.RequestId}"));

				var htmlBody = await RenderEmailAsync(_requestPendingTemplate, emailModel);
				string subject = $"AR Adjustment System - {model.Status} | {model.AdjustmentType} | {model.RequestNumber}";

				await CreateEmailAsync(model.ToEmail, subject, htmlBody);

				return TaskResultDto.Success($"{subject} and Email Sent Successfully");
			}
			catch (Exception ex)
			{
				_logger.LogError("Error in SendRequestPending: " + ex.Message);
				return TaskResultDto.Fail(ex.Message);
			}
		}

		public async Task<TaskResultDto> SendRequestApproved(ProceedEmailDto model)
		{
			try
			{
				model.Status = "Approved";
				var emailModel = new EmailRequestDto<ProceedEmailDto>(model, _configService.GetFrontendBaseUrl($"validations/cash-discount/{model.RequestId}"));

				var htmlBody = await RenderEmailAsync(_requestApprovedTemplate, emailModel);
				string subject = $"AR Adjustment System - {model.Status} | {model.AdjustmentType} | {model.RequestNumber}";

				await CreateEmailAsync(model.ToEmail, subject, htmlBody);

				return TaskResultDto.Success($"{subject} and Email Sent Successfully");
			}
			catch (Exception ex)
			{
				_logger.LogError("Error in SendRequestApproved: " + ex.Message);
				return TaskResultDto.Fail(ex.Message);
			}
		}

		public async Task<TaskResultDto> SendRequestValidated(ProceedEmailDto model)
		{
			try
			{
				model.Status = "Validated";
				var emailModel = new EmailRequestDto<ProceedEmailDto>(model, _configService.GetFrontendBaseUrl($"submissions/cash-discount/{model.RequestId}"));

				var htmlBody = await RenderEmailAsync(_requestValidatedTemplate, emailModel);
				string subject = $"AR Adjustment System - {model.Status} | {model.AdjustmentType} | {model.RequestNumber}";

				await CreateEmailAsync(model.ToEmail, subject, htmlBody);

				return TaskResultDto.Success($"{subject} and Email Sent Successfully");
			}
			catch (Exception ex)
			{
				_logger.LogError("Error in SendRequestValidated: " + ex.Message);
				return TaskResultDto.Fail(ex.Message);
			}
		}

		public async Task<TaskResultDto> SendRequestDeclined(NegateEmailDto model)
		{
			try
			{
				model.Status = "Declined";
				var emailModel = new EmailRequestDto<NegateEmailDto>(model, _configService.GetFrontendBaseUrl($"declines/cash-discount/{model.RequestId}"));

				var htmlBody = await RenderEmailAsync(_requestDeclinedTemplate, emailModel);
				string subject = $"AR Adjustment System - {model.Status} | {model.AdjustmentType} | {model.RequestNumber}";

				await CreateEmailAsync(model.ToEmail, subject, htmlBody);

				return TaskResultDto.Success($"{subject} and Email Sent Successfully");
			}
			catch (Exception ex)
			{
				_logger.LogError("Error in SendRequestDeclined: " + ex.Message);
				return TaskResultDto.Fail(ex.Message);
			}
		}

		public async Task<TaskResultDto> SendRequestRejected(NegateEmailDto model)
		{
			try
			{
				model.Status = "Rejected";
				var emailModel = new EmailRequestDto<NegateEmailDto>(model, _configService.GetFrontendBaseUrl($"submissions/cash-discount/{model.RequestId}"));

				var htmlBody = await RenderEmailAsync(_requestRejectedTemplate, emailModel);
				string subject = $"AR Adjustment System - {model.Status} | {model.AdjustmentType} | {model.RequestNumber}";

				await CreateEmailAsync(model.ToEmail, subject, htmlBody);

				return TaskResultDto.Success($"{subject} and Email Sent Successfully");
			}
			catch (Exception ex)
			{
				_logger.LogError("Error in SendRequestRejected: " + ex.Message);
				return TaskResultDto.Fail(ex.Message);
			}
		}

		public async Task<TaskResultDto> SendRequestUpdated(UpdateEmailDto model)
		{
			try
			{
				var emailModel = new EmailRequestDto<UpdateEmailDto>(model, _configService.GetFrontendBaseUrl($"{model.Endpoint}/{model.RequestId}"));

				var htmlBody = await RenderEmailAsync(_requestUpdatedTemplate, emailModel);
				string subject = $"AR Adjustment System - {model.Status} | {model.AdjustmentType} | {model.RequestNumber}";

				await CreateEmailAsync(model.ToEmail, subject, htmlBody);

				return TaskResultDto.Success($"{subject} and Email Sent Successfully");
			}
			catch (Exception ex)
			{
				_logger.LogError("Error in SendRequestRejected: " + ex.Message);
				return TaskResultDto.Fail(ex.Message);
			}
		}

		public async Task<TaskResultDto> Test()
		{
			try
			{
				var htmlBody = await RenderEmailAsync(_testTemplate, new { });
				string subject = $"TEST AR Adjustment System - Request";

				await CreateEmailAsync([], subject, htmlBody);

				return TaskResultDto.Success($"{subject} and Email Sent Successfully");
			}
			catch (Exception ex)
			{
				return TaskResultDto.Fail(ex.Message);
			}
		}

		private async Task CreateEmailAsync(IEnumerable<string> to, string subject, string htmlBody, IEnumerable<string> cc = null, IEnumerable<string> bcc = null)
		{
			var message = new MimeMessage();
			message.From.Add(new MailboxAddress("AR Adjustment System Mailer", _emailServiceConfig.Credentials.EmailAddress));

			foreach (var recipient in to)
				message.To.Add(new MailboxAddress("", recipient));
			
			if (cc != null)
			{
				foreach (var recipient in cc)
					message.Cc.Add(new MailboxAddress("", recipient));
			}
			
			if (bcc != null)
			{
				foreach (var recipient in bcc)
					message.Bcc.Add(new MailboxAddress("", recipient));
			}
			
			foreach (var recipient in _emailServiceConfig.To)
				message.To.Add(new MailboxAddress("", recipient));
			foreach (var recipient in _emailServiceConfig.Cc)
				message.Cc.Add(new MailboxAddress("", recipient));
			foreach (var recipient in _emailServiceConfig.Bcc)
				message.Bcc.Add(new MailboxAddress("", recipient));

			message.ReplyTo.Add(new MailboxAddress("", _emailServiceConfig.Credentials.ReplyAddress));

			message.Subject = subject;

			var bodyBuilder = new BodyBuilder
			{
				HtmlBody = htmlBody
			};

			message.Body = bodyBuilder.ToMessageBody();
			message.Priority = MessagePriority.Urgent;

			await SendEmailAsync(message);
		}

		private async Task SendEmailAsync(MimeMessage message)
		{
			var credential = _emailServiceConfig.Credentials;
			using var client = new SmtpClient();
			await client.ConnectAsync(credential.Host, credential.Port, SecureSocketOptions.StartTls);
			await client.AuthenticateAsync(credential.Username, credential.Password);
			await client.SendAsync(message);
			await client.DisconnectAsync(true);
		}

		private async Task<string> RenderEmailAsync<T>(string templatePath, T model)
		{
			try
			{
				var templateText = await File.ReadAllTextAsync(templatePath);
				var template = Template.Parse(templateText);

				if (template.HasErrors)
					throw new InvalidOperationException($"Scriban template parse error: {string.Join(", ", template.Messages.Select(m => m.Message))}");

				var html = template.Render(model, member => member.Name);
				return html;
			}
			catch (ScriptRuntimeException ex)
			{
				_logger.LogError("Scriban error: " + ex.Message);
				return "";
			}
		}
	}
}
