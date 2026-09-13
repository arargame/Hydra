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
        public string? PasswordHash { get; set; } = null;
        public Guid? PasswordResetValidationToken { get; set; } = null;

        public SystemUserViewDTO()
        {
            SetControllerName("SystemUser");
        }

        public override DTO LoadConfigurations()
        {
            // 1. Email (ListView, CollectionView, DetailsView, EditView, CreateView)
            SetConfigurationsViaStringPropertyInfo(
                propertyInfo: ReflectionHelper.GetPropertyOf<SystemUserViewDTO>(x => x.Email!),
                displayName: "Email",
                attributeToFilter: new AttributeToFilter(nameof(ContainsFilter)),
                useToCreateCollectionViewConfiguration: true
            );

            // 2. Name (ListView, CollectionView, DetailsView, EditView, CreateView)
            SetConfigurationsViaStringPropertyInfo(
                propertyInfo: ReflectionHelper.GetPropertyOf<SystemUserViewDTO>(x => x.Name!),
                displayName: "Name",
                attributeToFilter: new AttributeToFilter(nameof(ContainsFilter)),
                useToCreateCollectionViewConfiguration: true
            );

            // 3. Description (ListView, DetailsView, EditView, CreateView) - CollectionView YOK
            SetConfigurationsViaStringPropertyInfo(
                propertyInfo: ReflectionHelper.GetPropertyOf<SystemUserViewDTO>(x => x.Description!),
                displayName: "Description",
                attributeToFilter: new AttributeToFilter(nameof(ContainsFilter)),
                htmlElementTypeInCreationAndEdit: HtmlElementType.TextArea,
                useToCreateCollectionViewConfiguration: false
            );

            // 4. NickName (ListView, CollectionView, DetailsView, EditView, CreateView)
            SetConfigurationsViaStringPropertyInfo(
                propertyInfo: ReflectionHelper.GetPropertyOf<SystemUserViewDTO>(x => x.NickName!),
                displayName: "Nickname",
                attributeToFilter: new AttributeToFilter(nameof(ContainsFilter)),
                useToCreateCollectionViewConfiguration: true
            );

            // 5. EmailConfirmed (ListView, CollectionView, DetailsView, EditView) - CreateView YOK
            SetConfigurationsViaPropertyInfo(
                propertyInfo: ReflectionHelper.GetPropertyOf<SystemUserViewDTO>(x => x.EmailConfirmed),
                configurations: new List<IConfiguration>
                {
                    new ListViewConfiguration(
                        toFilter: new AttributeToFilter(nameof(EqualFilter)),
                        toOrder: new AttributeToOrder(isOrderable: true),
                        elementType: HtmlElementType.DropdownList)
                        .AlsoUseToCreateCollectionViewConfiguration(),

                    new EditViewConfiguration(elementType: HtmlElementType.DropdownList),

                    new DetailsViewConfiguration()
                },
                displayName: "Email Confirmed"
            );

            // 6. PhoneNumberConfirmed (ListView, CollectionView, DetailsView, EditView) - CreateView YOK
            SetConfigurationsViaPropertyInfo(
                propertyInfo: ReflectionHelper.GetPropertyOf<SystemUserViewDTO>(x => x.PhoneNumberConfirmed),
                configurations: new List<IConfiguration>
                {
                    new ListViewConfiguration(
                        toFilter: new AttributeToFilter(nameof(EqualFilter)),
                        toOrder: new AttributeToOrder(isOrderable: true),
                        elementType: HtmlElementType.DropdownList)
                        .AlsoUseToCreateCollectionViewConfiguration(),

                    new EditViewConfiguration(elementType: HtmlElementType.DropdownList),

                    new DetailsViewConfiguration()
                },
                displayName: "Phone Confirmed"
            );

            // 7. PhoneNumberId (ListView, CollectionView, DetailsView, EditView) - CreateView YOK
            SetConfigurationsViaPropertyInfo(
                propertyInfo: ReflectionHelper.GetPropertyOf<SystemUserViewDTO>(x => x.PhoneNumberId!),
                configurations: new List<IConfiguration>
                {
                    new ListViewConfiguration(
                        toFilter: new AttributeToFilter(nameof(EqualFilter)),
                        toOrder: new AttributeToOrder(isOrderable: true))
                        .AlsoUseToCreateCollectionViewConfiguration()
                        .AlsoUseToCreateLookupViewConfiguration(),

                    new EditViewConfiguration(),

                    new DetailsViewConfiguration()
                },
                displayName: "Phone Number Id"
            );

            // 8. IsActive (ListView, CollectionView, DetailsView, EditView) - CreateView YOK
            SetConfigurationsViaPropertyInfo(
                propertyInfo: ReflectionHelper.GetPropertyOf<SystemUserViewDTO>(x => x.IsActive),
                configurations: new List<IConfiguration>
                {
                    new ListViewConfiguration(
                        toFilter: new AttributeToFilter(nameof(EqualFilter)),
                        toOrder: new AttributeToOrder(isOrderable: true),
                        elementType: HtmlElementType.DropdownList)
                        .AlsoUseToCreateCollectionViewConfiguration(),

                    new EditViewConfiguration(elementType: HtmlElementType.DropdownList),

                    new DetailsViewConfiguration()
                },
                displayName: "Active"
            );

            // 9. AddedDate (ListView, CollectionView, DetailsView) - CreateView/EditView YOK
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

            // 10. ModifiedDate (ListView, CollectionView, DetailsView) - CreateView/EditView YOK
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

            // 11. PasswordHash (DetailsView) - Sadece DetailsView
            SetConfigurationsViaPropertyInfo(
                propertyInfo: ReflectionHelper.GetPropertyOf<SystemUserViewDTO>(x => x.PasswordHash!),
                configurations: new List<IConfiguration>
                {
                    new DetailsViewConfiguration()
                },
                displayName: "Password Hash"
            );

            // 12. PasswordResetValidationToken (CollectionView, DetailsView) - ListView YOK
            SetConfigurationsViaPropertyInfo(
                propertyInfo: ReflectionHelper.GetPropertyOf<SystemUserViewDTO>(x => x.PasswordResetValidationToken!),
                configurations: new List<IConfiguration>
                {
                    new ListViewConfiguration(
                        toFilter: new AttributeToFilter(nameof(EqualFilter)),
                        toOrder: new AttributeToOrder(isOrderable: true))
                        .AlsoUseToCreateCollectionViewConfiguration(useToCreateCollectionViewConfiguration: true, thenRemoveThis: true),

                    new DetailsViewConfiguration()
                },
                displayName: "Password Reset Token"
            );

            SortByWrittenOrder();

            return this;
        }
    }
}
