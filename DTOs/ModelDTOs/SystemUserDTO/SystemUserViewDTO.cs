using Hydra.AccessManagement;
using Hydra.DataModels;
using Hydra.DataModels.Filter;
using Hydra.DTOs.ViewConfigurations;
using Hydra.DTOs.ViewDTOs;
using Hydra.Utils;
using System;
using System.Collections.Generic;

namespace Hydra.DTOs.ModelDTOs.SystemUserDTO
{
    [RegisterAsViewDTO(nameof(SystemUser))]
    public class SystemUserViewDTO : ViewDTO
    {
        public string? Email { get; set; } = null;
        public string? NickName { get; set; } = null;
        public bool EmailConfirmed { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public Guid? PhoneNumberId { get; set; } = null;

        public SystemUserViewDTO()
        {
            SetControllerName("SystemUser");
        }

        public override DTO LoadConfigurations()
        {
            // 1. Email
            SetConfigurationsViaStringPropertyInfo(
                propertyInfo: ReflectionHelper.GetPropertyOf<SystemUserViewDTO>(x => x.Email!),
                displayName: "Email",
                attributeToFilter: new AttributeToFilter(nameof(ContainsFilter))
            );

            // 2. Name
            SetConfigurationsViaStringPropertyInfo(
                propertyInfo: ReflectionHelper.GetPropertyOf<SystemUserViewDTO>(x => x.Name!),
                displayName: "Name",
                attributeToFilter: new AttributeToFilter(nameof(ContainsFilter))
            );

            // 3. Description
            SetConfigurationsViaStringPropertyInfo(
                propertyInfo: ReflectionHelper.GetPropertyOf<SystemUserViewDTO>(x => x.Description!),
                displayName: "Description",
                attributeToFilter: new AttributeToFilter(nameof(ContainsFilter)),
                htmlElementTypeInCreationAndEdit: HtmlElementType.TextArea
            );

            // 4. NickName
            SetConfigurationsViaStringPropertyInfo(
                propertyInfo: ReflectionHelper.GetPropertyOf<SystemUserViewDTO>(x => x.NickName!),
                displayName: "Nickname",
                attributeToFilter: new AttributeToFilter(nameof(ContainsFilter))
            );

            // 5. EmailConfirmed
            SetConfigurationsViaBooleanPropertyInfo(
                propertyInfo: ReflectionHelper.GetPropertyOf<SystemUserViewDTO>(x => x.EmailConfirmed),
                displayName: "Email Confirmed"
            );

            // 6. PhoneNumberConfirmed
            SetConfigurationsViaBooleanPropertyInfo(
                propertyInfo: ReflectionHelper.GetPropertyOf<SystemUserViewDTO>(x => x.PhoneNumberConfirmed),
                displayName: "Phone Confirmed"
            );

            // 7. PhoneNumberId
            SetConfigurationsViaPropertyInfo(
                propertyInfo: ReflectionHelper.GetPropertyOf<SystemUserViewDTO>(x => x.PhoneNumberId!),
                configurations: new List<IConfiguration>
                {
                    new ListViewConfiguration(
                        toFilter: new AttributeToFilter(nameof(EqualFilter)),
                        toOrder: new AttributeToOrder(isOrderable: true))
                        .AlsoUseToCreateCollectionViewConfiguration()
                        .AlsoUseToCreateLookupViewConfiguration(),

                    new DetailsViewConfiguration()
                },
                displayName: "Phone Number Id"
            );

            // 8. IsActive
            SetConfigurationsViaBooleanPropertyInfo(
                propertyInfo: ReflectionHelper.GetPropertyOf<SystemUserViewDTO>(x => x.IsActive),
                displayName: "Active"
            );

            // 9. AddedDate
            SetConfigurationsViaPropertyInfo(
                propertyInfo: ReflectionHelper.GetPropertyOf<SystemUserViewDTO>(x => x.AddedDate),
                configurations: new List<IConfiguration>
                {
                    new ListViewConfiguration(
                        toFilter: new AttributeToFilter(nameof(BetweenFilter)),
                        toOrder: new AttributeToOrder(isOrderable: true))
                        .AlsoUseToCreateCollectionViewConfiguration(),

                    new DetailsViewConfiguration()
                },
                displayName: "Added Date"
            );

            // 10. ModifiedDate
            SetConfigurationsViaPropertyInfo(
                propertyInfo: ReflectionHelper.GetPropertyOf<SystemUserViewDTO>(x => x.ModifiedDate),
                configurations: new List<IConfiguration>
                {
                    new ListViewConfiguration(
                        toFilter: new AttributeToFilter(nameof(BetweenFilter)),
                        toOrder: new AttributeToOrder(isOrderable: true)),

                    new DetailsViewConfiguration()
                },
                displayName: "Modified Date"
            );

            SortByWrittenOrder();

            return this;
        }
    }
}
