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

		private readonly string _approversTemplate = Path.Combine("App_Code", "Scriban", "ToApproverTemplate.sbn");

		private readonly EmailServiceConfig _emailServiceConfig;

		public EmailService(IOptions<EmailServiceConfig> emailServiceOption)
		{
			_emailServiceConfig = emailServiceOption.Value;
		}

		public async Task<TaskResultDto> SendToApprover()
		{
			try
			{
				//CaseCreateEmailDetails emailDetails = new CaseCreateEmailDetails();
				//emailDetails.BaseUrl = _baseUrl;
				//emailDetails.Model = emailViewModel;

				var htmlBody = await RenderEmailAsync(_approversTemplate, new {});

				//await CreateEmailAsync([], [emailDetails.Model.SenderEmail], $"TEST PCSv2 - Request from {emailDetails.Model.BranchName}", htmlBody, _credentials);

				return TaskResultDto.Success("Case Created and Email Sent Successfully");
			}
			catch (Exception ex)
			{
				return TaskResultDto.Fail(ex.Message);
			}
		}

		public async Task<TaskResultDto> Test()
		{
			try
			{
				var htmlBody = await RenderEmailAsync(_approversTemplate, new { });

				await CreateEmailAsync([], $"TEST AR Adjustment System - Request", htmlBody);

				return TaskResultDto.Success("Case Created and Email Sent Successfully");
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
				Console.WriteLine("Scriban error: " + ex.Message);
				return "";
			}
		}

	}
}
