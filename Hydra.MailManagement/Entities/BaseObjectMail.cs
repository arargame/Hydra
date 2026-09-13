using Hydra.Core;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hydra.MailManagement.Entities
{
    public class BaseObjectMail : BaseObject<BaseObjectMail>
    {
        public Guid MailId { get; set; }

        public Guid EntityId { get; set; }

        public string EntityType { get; set; } = string.Empty;

        [ForeignKey(nameof(MailId))]
        public virtual Mail? Mail { get; set; }

        public BaseObjectMail()
        {
        }

        public BaseObjectMail(Guid mailId, Guid entityId, string entityType)
        {
            MailId = mailId;
            EntityId = entityId;
            EntityType = entityType;
            Name = $"{entityType}:{entityId}->{mailId}";
        }
    }
}
