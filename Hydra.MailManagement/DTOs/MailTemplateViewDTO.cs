using Hydra.DTOs;
using Hydra.DTOs.ViewDTOs;
using Hydra.MailManagement.Entities;
using Hydra.Utils;

namespace Hydra.MailManagement.DTOs
{
    [RegisterAsViewDTO(nameof(MailTemplate))]
    public class MailTemplateViewDTO : ViewDTO
    {
        public string Code { get; set; } = string.Empty;

        public string Subject { get; set; } = string.Empty;

        public string? Parameters { get; set; }

        public MailTemplateViewDTO()
        {
            SetControllerName("MailTemplate");
        }

        public override DTO LoadConfigurations()
        {
            SetConfigurationsForBaseObjectMembers();

            SetConfigurationsViaStringPropertyInfo(
                propertyInfo: ReflectionHelper.GetPropertyOf(type: GetType(), propertyName: nameof(MailTemplateViewDTO.Code)),
                displayName: nameof(MailTemplateViewDTO.Code));

            SetConfigurationsViaStringPropertyInfo(
                propertyInfo: ReflectionHelper.GetPropertyOf(type: GetType(), propertyName: nameof(MailTemplateViewDTO.Subject)),
                displayName: nameof(MailTemplateViewDTO.Subject));

            return base.LoadConfigurations();
        }
    }
}
