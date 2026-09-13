using Hydra.Core;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hydra.MailManagement.Entities
{
    public class MailAttachment : BaseObject<MailAttachment>
    {
        public Guid? MailId { get; set; }

        public string FileName { get; set; } = string.Empty;

        public string Extension { get; set; } = string.Empty;

        public string? ContentType { get; set; }

        public byte[]? Data { get; set; }

        public long FileSize { get; set; }

        [ForeignKey(nameof(MailId))]
        public virtual Mail? Mail { get; set; }

        public MailAttachment()
        {
        }

        public MailAttachment(string fileName, string extension, byte[] data, string? contentType = null)
        {
            FileName = fileName;
            Extension = extension.StartsWith('.') ? extension : $".{extension}";
            Data = data;
            FileSize = data?.LongLength ?? 0;
            ContentType = contentType;
            Name = $"{fileName}{Extension}";
        }
    }
}
