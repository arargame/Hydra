using Hydra.Core;
using Hydra.DI;
using Hydra.Http;
using Hydra.MailManagement.DTOs;
using Hydra.MailManagement.Engine;
using Hydra.MailManagement.Entities;
using Hydra.MailManagement.Enums;
using Hydra.Services.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Hydra.MailManagement.Services
{
    [RegisterAsService(typeof(IService<Mail>))]
    public class MailService : Service<Mail>
    {
        private readonly IMailEngineService? _mailEngineService;
        private readonly Lazy<MailTemplateService>? _mailTemplateService;

        public MailService(ServiceInjector injector) : base(injector)
        {
            _mailEngineService = injector.ServiceProvider.GetService<IMailEngineService>();
            _mailTemplateService = injector.GetServiceLazy<MailTemplateService>();
        }

        /// <summary>
        /// Sends an existing persisted Mail by its Id.
        /// Updates status to Queued -> Sent or Failed, records error messages, and logs errors.
        /// </summary>
        public async Task<IResponseObject> SendAsync(Guid mailId, CancellationToken cancellationToken = default)
        {
            var response = new ResponseObject()
                .SetActionName("Send")
                .SetId(mailId)
                .UseDefaultMessages();

            if (_mailEngineService == null)
            {
                response.SetSuccess(false);
                response.AddExtraMessage(new ResponseObjectMessage("Configuration", "IMailEngineService is not registered in the DI container.", false));
                return response;
            }

            var mail = await GetByIdAsync(mailId);
            if (mail == null)
            {
                response.SetSuccess(false);
                response.AddExtraMessage(new ResponseObjectMessage("NotFound", $"Mail with id '{mailId}' was not found.", false));
                return response;
            }

            // Mark as Queued before dispatch
            mail.ChangeStatus(MailStatus.Queued);
            await UpdateAsync(mail);

            // Physical dispatch via transport engine
            var (success, errorMessage) = await _mailEngineService.SendMailAsync(mail, cancellationToken);

            if (success)
            {
                mail.ChangeStatus(MailStatus.Sent);
            }
            else
            {
                mail.ChangeStatus(MailStatus.Failed, errorMessage);

                await SaveErrorLogAsync(
                    description: $"Mail dispatch failed for '{mail.To}' with Subject '{mail.Subject}': {errorMessage}",
                    entityId: mail.Id,
                    processType: LogProcessType.Update);
            }

            await UpdateAsync(mail);

            response.SetSuccess(success);
            response.SetData(mail);

            var resultMsg = success ? "Email successfully sent." : $"Email dispatch failed: {errorMessage}";
            response.AddExtraMessage(new ResponseObjectMessage("SendResult", resultMsg, success));

            return response;
        }

        /// <summary>
        /// Saves a Mail to database and immediately dispatches it.
        /// </summary>
        public async Task<IResponseObject> QueueAndSendAsync(Mail mail, object? model = null, CancellationToken cancellationToken = default)
        {
            if (mail.MailTemplate != null && model != null)
            {
                mail.ApplyTemplate(mail.MailTemplate, model);
            }
            else if (mail.MailTemplateId.HasValue && model != null && _mailTemplateService != null)
            {
                var template = await _mailTemplateService.Value.GetByIdAsync(mail.MailTemplateId.Value);
                if (template != null)
                {
                    mail.ApplyTemplate(template, model);
                }
            }

            var createResponse = await CreateAsync(mail);
            if (!createResponse.Success)
            {
                return createResponse;
            }

            return await SendAsync(mail.Id, cancellationToken);
        }

        /// <summary>
        /// Quick send helper using a simplified DTO contract.
        /// </summary>
        public async Task<IResponseObject> SendQuickAsync(QuickSendMailDTO dto, CancellationToken cancellationToken = default)
        {
            var mail = new Mail
            {
                To = dto.To,
                Subject = dto.Subject,
                Body = dto.Body ?? string.Empty,
                IsBodyHtml = dto.IsBodyHtml
            };

            if (!string.IsNullOrWhiteSpace(dto.TemplateCode) && _mailTemplateService != null)
            {
                var template = await _mailTemplateService.Value.GetByCodeAsync(dto.TemplateCode);
                if (template != null)
                {
                    mail.ApplyTemplate(template, dto.TemplateParameters);
                }
            }

            if (dto.EntityId.HasValue && !string.IsNullOrWhiteSpace(dto.EntityType))
            {
                mail.AttachEntity(dto.EntityId.Value, dto.EntityType);
            }

            return await QueueAndSendAsync(mail, null, cancellationToken);
        }

        /// <summary>
        /// Retries dispatching failed emails that have not exceeded the max retry count.
        /// </summary>
        public async Task<List<IResponseObject>> RetryFailedMailsAsync(int maxRetryCount = 3, CancellationToken cancellationToken = default)
        {
            var failedMails = await FilterWithLinq(m => m.Status == MailStatus.Failed && m.RetryCount < maxRetryCount && m.IsActive)
                .ToListAsync(cancellationToken);

            var results = new List<IResponseObject>();

            foreach (var mail in failedMails)
            {
                var res = await SendAsync(mail.Id, cancellationToken);
                results.Add(res);
            }

            return results;
        }
    }
}
