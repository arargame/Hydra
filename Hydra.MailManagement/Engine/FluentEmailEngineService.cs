using FluentEmail.Core;
using FluentEmail.Core.Models;
using Hydra.MailManagement.Configuration;
using Hydra.MailManagement.Entities;
using Microsoft.Extensions.Options;

namespace Hydra.MailManagement.Engine
{
    public class FluentEmailEngineService : IMailEngineService
    {
        private readonly IFluentEmailFactory _fluentEmailFactory;
        private readonly HydraMailOptions _options;

        public FluentEmailEngineService(
            IFluentEmailFactory fluentEmailFactory,
            IOptions<HydraMailOptions> options)
        {
            _fluentEmailFactory = fluentEmailFactory;
            _options = options.Value;
        }

        public async Task<(bool Success, string? ErrorMessage)> SendMailAsync(Mail mail, CancellationToken cancellationToken = default)
        {
            try
            {
                var fluentEmail = _fluentEmailFactory.Create();

                // 1. From address handling
                var fromEmail = !string.IsNullOrWhiteSpace(mail.From) ? mail.From : _options.DefaultFromEmail;
                var fromName = !string.IsNullOrWhiteSpace(mail.FromName) ? mail.FromName : _options.DefaultFromName;
                fluentEmail.SetFrom(fromEmail, fromName);

                // 2. Recipients
                var toList = mail.GetRecipientList(mail.To);
                foreach (var to in toList)
                {
                    fluentEmail.To(to);
                }

                var ccList = mail.GetRecipientList(mail.Cc);
                foreach (var cc in ccList)
                {
                    fluentEmail.CC(cc);
                }

                var bccList = mail.GetRecipientList(mail.Bcc);
                foreach (var bcc in bccList)
                {
                    fluentEmail.BCC(bcc);
                }

                // 3. Subject and Body
                fluentEmail.Subject(mail.Subject ?? string.Empty);
                fluentEmail.Body(mail.Body ?? string.Empty, mail.IsBodyHtml);

                // 4. Attachments
                if (mail.Attachments != null && mail.Attachments.Count > 0)
                {
                    foreach (var att in mail.Attachments)
                    {
                        if (att.Data != null && att.Data.Length > 0)
                        {
                            var stream = new MemoryStream(att.Data);
                            var attachment = new Attachment
                            {
                                Data = stream,
                                Filename = $"{att.FileName}{att.Extension}",
                                ContentType = att.ContentType ?? "application/octet-stream"
                            };
                            fluentEmail.Attach(attachment);
                        }
                    }
                }

                // 5. Send
                var response = await fluentEmail.SendAsync(cancellationToken);

                if (response.Successful)
                {
                    return (true, null);
                }

                var errorCombined = response.ErrorMessages != null && response.ErrorMessages.Count > 0
                    ? string.Join("; ", response.ErrorMessages)
                    : "Unknown error from email sender provider.";

                return (false, errorCombined);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }
}
