using Hydra.Core;
using Hydra.MailManagement.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hydra.MailManagement.Entities
{
    public class Mail : BaseObject<Mail>
    {
        public MailStatus Status { get; set; } = MailStatus.Draft;

        public string? From { get; set; }

        public string? FromName { get; set; }

        public string To { get; set; } = string.Empty;

        public string? Cc { get; set; }

        public string? Bcc { get; set; }

        public string Subject { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;

        public bool IsBodyHtml { get; set; } = true;

        public Guid? MailTemplateId { get; set; }

        public DateTime? SentDate { get; set; }

        public DateTime? FailedDate { get; set; }

        public string? ErrorMessage { get; set; }

        public int RetryCount { get; set; } = 0;

        [ForeignKey(nameof(MailTemplateId))]
        public virtual MailTemplate? MailTemplate { get; set; }

        public virtual ICollection<MailAttachment> Attachments { get; set; } = new List<MailAttachment>();

        public virtual ICollection<BaseObjectMail> BaseObjectMails { get; set; } = new List<BaseObjectMail>();

        public Mail()
        {
        }

        public Mail(string to, string subject, string body, bool isBodyHtml = true)
        {
            To = to;
            Subject = subject;
            Body = body;
            IsBodyHtml = isBodyHtml;
            Name = subject;
        }

        public Mail(string to, MailTemplate template, object? model = null, bool isBodyHtml = true)
        {
            To = to;
            IsBodyHtml = isBodyHtml;
            ApplyTemplate(template, model);
        }

        public Mail ApplyTemplate(MailTemplate template, object? model = null)
        {
            MailTemplate = template;
            MailTemplateId = template.Id;
            Subject = template.RenderSubject(model);
            Body = template.RenderBody(model);
            Name = Subject;
            return this;
        }

        public Mail SetFrom(string from, string? fromName = null)
        {
            From = from;
            FromName = fromName;
            return this;
        }

        public Mail SetTo(string to)
        {
            To = to;
            return this;
        }

        public Mail SetCc(string? cc)
        {
            Cc = cc;
            return this;
        }

        public Mail SetBcc(string? bcc)
        {
            Bcc = bcc;
            return this;
        }

        public Mail SetSubject(string subject)
        {
            Subject = subject;
            Name = subject;
            return this;
        }

        public Mail SetBody(string body, bool isHtml = true)
        {
            Body = body;
            IsBodyHtml = isHtml;
            return this;
        }

        public Mail AddAttachment(string fileName, string extension, byte[] data, string? contentType = null)
        {
            var attachment = new MailAttachment(fileName, extension, data, contentType)
            {
                MailId = Id
            };
            Attachments.Add(attachment);
            return this;
        }

        public Mail AttachEntity(Guid entityId, string entityType)
        {
            var link = new BaseObjectMail(Id, entityId, entityType);
            BaseObjectMails.Add(link);
            return this;
        }

        public Mail ChangeStatus(MailStatus status, string? error = null)
        {
            Status = status;

            if (status == MailStatus.Sent)
            {
                SentDate = DateTime.UtcNow;
                ErrorMessage = null;
            }
            else if (status == MailStatus.Failed)
            {
                FailedDate = DateTime.UtcNow;
                ErrorMessage = error;
                RetryCount++;
            }

            return this;
        }

        public List<string> GetRecipientList(string? rawList)
        {
            if (string.IsNullOrWhiteSpace(rawList))
                return new List<string>();

            return rawList
                .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();
        }
    }
}
