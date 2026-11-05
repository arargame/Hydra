using Hydra.AccessManagement;
using Hydra.DataModels;
using Hydra.DataModels.Filter;
using Hydra.DTOs.ViewConfigurations;
using Hydra.DTOs.ViewDTOs;
using Hydra.Utils;

namespace Hydra.DTOs.Preferences
{
    [RegisterAsViewDTO("UserViewPreference")]
    public class UserViewPreferenceDTO : ViewDTO
    {
        public Guid? SystemUserId { get; set; }
        public string? SystemUser_Name { get; set; }
        public Guid? SystemUser_Id { get; set; }
        public string ViewKey { get; set; } = string.Empty;
        public string? PresetName { get; set; }
        public bool IsDefault { get; set; }
        public string ConfigurationJson { get; set; } = string.Empty;

        public UserViewPreferenceDTO()
        {
            SetControllerName("UserViewPreference");
        }

        public override DTO LoadConfigurations()
        {
            SetConfigurationsViaStringPropertyInfo(
                propertyInfo: ReflectionHelper.GetPropertyOf<UserViewPreferenceDTO>(x => x.ViewKey),
                displayName: "View Key",
                attributeToFilter: new AttributeToFilter(nameof(EqualFilter))
            );

            SetConfigurationsViaStringPropertyInfo(
                propertyInfo: ReflectionHelper.GetPropertyOf<UserViewPreferenceDTO>(x => x.PresetName),
                displayName: "Preset Name",
                attributeToFilter: new AttributeToFilter(nameof(ContainsFilter))
            );

            SetConfigurationsViaBooleanPropertyInfo(
                propertyInfo: ReflectionHelper.GetPropertyOf<UserViewPreferenceDTO>(x => x.IsDefault),
                displayName: "Is Default"
            );

            SetConfigurationsViaStringPropertyInfo(
                propertyInfo: ReflectionHelper.GetPropertyOf<UserViewPreferenceDTO>(x => x.ConfigurationJson),
                displayName: "Configuration JSON",
                htmlElementTypeInCreationAndEdit: HtmlElementType.TextArea
            );

            SetConfigurationsForNavigations<UserViewPreferenceDTO, SystemUser>(
                currentDto => currentDto.SystemUserId!,
                systemUser => systemUser.Name!,
                displayName: "User"
            );

            SetConfigurationsForBaseObjectMembers();

            return this;
        }
    }
}
