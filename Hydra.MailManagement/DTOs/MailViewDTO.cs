using Hydra.DTOs;
using Hydra.DTOs.ViewDTOs;
using Hydra.MailManagement.Entities;
using Hydra.MailManagement.Enums;
using Hydra.Utils;

namespace Hydra.MailManagement.DTOs
{
    [RegisterAsViewDTO(nameof(Mail))]
    public class MailViewDTO : ViewDTO
    {
        public MailStatus Status { get; set; } = MailStatus.Draft;

        public string? From { get; set; }

        public string To { get; set; } = string.Empty;

        public string? Cc { get; set; }

        public string? Bcc { get; set; }

        public string Subject { get; set; } = string.Empty;

        public DateTime? SentDate { get; set; }

        public string? ErrorMessage { get; set; }

        public MailViewDTO()
        {
            SetControllerName("Mail");
        }

        public override DTO LoadConfigurations()
        {
            SetConfigurationsForBaseObjectMembers();

            SetConfigurationsViaStringPropertyInfo(
                propertyInfo: ReflectionHelper.GetPropertyOf(type: GetType(), propertyName: nameof(MailViewDTO.To)),
                displayName: nameof(MailViewDTO.To));

            SetConfigurationsViaStringPropertyInfo(
                propertyInfo: ReflectionHelper.GetPropertyOf(type: GetType(), propertyName: nameof(MailViewDTO.Subject)),
                displayName: nameof(MailViewDTO.Subject));

            return base.LoadConfigurations();
        }
    }
}
