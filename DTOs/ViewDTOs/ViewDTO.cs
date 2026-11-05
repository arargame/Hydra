using Hydra.Core;
using Hydra.DataModels.Filter;
using Hydra.DataModels;
using Hydra.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Hydra.DTOs.ViewConfigurations;

namespace Hydra.DTOs.ViewDTOs
{
    public enum ViewType
    {
        None,
        ListView,
        CollectionView,
        LookupView,
        CreateView,
        EditView,
        DetailsView,
        Others
    }

    public interface ICollectionHandlerDTO
    {
        List<CollectionViewDTO> Collections { get; set; }
        DTO LoadCollections();
    }

    public interface IViewDTO : IDTO, ICollectionHandlerDTO
    {
        string? Description { get; set; }

        DateTime AddedDate { get; set; }
        DateTime ModifiedDate { get; set; }

        bool IsActive { get; set; }

        bool IsHidden { get; set; }
        string? ControllerName { get; set; }
        string? ActionName { get; set; }
        string CreateLink { get; }

        string DetailsLink { get; }
        string EditLink { get; }
        string DeleteLink { get; }

        IViewDTO SetControllerName(string controllerName);
        IViewDTO SetActionName(string actionName);
    }

    public abstract class ViewDTO : DTO, IViewDTO
    {
        public string? Description { get; set; } = null;

        public DateTime AddedDate { get; set; }

        public DateTime ModifiedDate { get; set; }

        public bool IsActive { get; set; }

        public bool IsHidden { get; set; }

        public virtual string? ControllerName { get; set; } = null;
        public string? ActionName { get; set; } = null;

        private string? createLink = null;
        public virtual string CreateLink => createLink ??= string.Format("{0}/CreateView", ControllerName);

        private string? detailsLink = null;
        public virtual string DetailsLink => detailsLink ??= string.Format("{0}/DetailsView/{1}", ControllerName, Id);

        private string? editLink = null;
        public virtual string EditLink => editLink ??= string.Format("{0}/EditView/{1}", ControllerName, Id);

        private string? deleteLink = null;
        public virtual string DeleteLink => deleteLink ??= string.Format("{0}/DeleteView/{1}", ControllerName, Id);

        public List<IConfiguration> Configurations = new List<IConfiguration>();

        public List<CollectionViewDTO> Collections { get; set; } = new();

        public ViewDTO() { }

        public IViewDTO SetControllerName(string controllerName)
        {
            ControllerName = controllerName;
            return this;
        }

        public IViewDTO SetActionName(string actionName)
        {
            ActionName = actionName;
            return this;
        }

        public ViewDTO SetConfigurationsViaPropertyInfo(PropertyInfo? propertyInfo,
                                                List<IConfiguration> configurations,
                                                string? displayName = null,
                                                NavigationColumnInfo? navigation = null)
        {
            configurations.ForEach(c => c.SetPropertyInfo(propertyInfo));

            if (displayName != null)
                configurations.ForEach(c => c.SetDisplayName(displayName));

            if (navigation != null)
                configurations.ForEach(c => c.SetNavigationColumnInfo(navigation));

            var listViewConfiguration = (configurations.Where(c => c.ViewType == ViewType.ListView).FirstOrDefault() as ListViewConfiguration);

            if (listViewConfiguration != null)
            {
                if (listViewConfiguration.ToCreateCollectionViewConfiguration)
                {
                    var collectionViewConfiguration = listViewConfiguration.ConvertToAnotherViewConfiguration(ViewType.CollectionView);

                    configurations.Add(collectionViewConfiguration);

                    if (listViewConfiguration.ThenRemoveThis)
                        configurations.Remove(listViewConfiguration);
                }

                if (listViewConfiguration.ToCreateLookupViewConfiguration)
                {
                    var lookupViewConfiguration = listViewConfiguration.ConvertToAnotherViewConfiguration(ViewType.LookupView);

                    configurations.Add(lookupViewConfiguration);

                    if (listViewConfiguration.ThenRemoveThis)
                        configurations.Remove(listViewConfiguration);
                }
            }

            foreach (var configuration in configurations)
            {
                configuration.SetPropertyInfo(propertyInfo)
                            .SetTableName(ControllerName);
            }

            Configurations.AddRange(configurations);

            return this;
        }


        public ViewDTO SetConfigurationsViaEnumPropertyInfo(PropertyInfo? propertyInfo,
                                                string? displayName = null,
                                                NavigationColumnInfo? navigation = null,
                                                object? defaultValue = null,
                                                bool useToCreateCollectionViewConfiguration = true,
                                                bool useToCreateLookupViewConfiguration = true)
        {
            var configurations = new List<IConfiguration>()
            {
                   new CreateViewConfiguration(elementType: HtmlElementType.DropdownList,defaultValue:defaultValue),

                   //Enum kolonları da string/boolean kolonlar gibi CollectionView ve LookupView
                   //konfigürasyonu üretmeli; aksi halde bir entity'nin master-detail sekmesinde
                   //(CollectionView) durum/tip gibi enum kolonları hiç görünmez.
                   new ListViewConfiguration(toFilter: new AttributeToFilter(priority: 0, typeName: nameof(EqualFilter)),
                                            toOrder: new AttributeToOrder(isOrderable: true),
                                            elementType: HtmlElementType.DropdownList)
                        .AlsoUseToCreateCollectionViewConfiguration(useToCreateCollectionViewConfiguration)
                        .AlsoUseToCreateLookupViewConfiguration(useToCreateLookupViewConfiguration),

                   new EditViewConfiguration(elementType: HtmlElementType.DropdownList),

                   new DetailsViewConfiguration(),
            };

            return SetConfigurationsViaPropertyInfo(propertyInfo: propertyInfo,
                                                    configurations: configurations,
                                                    displayName: displayName,
                                                    navigation: navigation);
        }

        public ViewDTO SetConfigurationsViaStringPropertyInfo(PropertyInfo? propertyInfo,
                                                string? displayName = null,
                                                NavigationColumnInfo? navigation = null,
                                                AttributeToFilter? attributeToFilter = null,
                                                HtmlElementType htmlElementTypeInFiltering = HtmlElementType.Input,
                                                HtmlElementType htmlElementTypeInCreationAndEdit = HtmlElementType.Input,
                                                object? defaultValueInCreation = null,
                                                bool useToCreateCollectionViewConfiguration = true,
                                                bool useToCreateLookupViewConfiguration = true)
        {
            var configurations = new List<IConfiguration>()
            {
                new CreateViewConfiguration(elementType:htmlElementTypeInCreationAndEdit,defaultValue:defaultValueInCreation),

                new ListViewConfiguration(toFilter: attributeToFilter ?? new AttributeToFilter(typeName: nameof(ContainsFilter)),
                                            toOrder:new AttributeToOrder(isOrderable:true),
                                            elementType:htmlElementTypeInFiltering)
                    .AlsoUseToCreateCollectionViewConfiguration(useToCreateCollectionViewConfiguration)
                    .AlsoUseToCreateLookupViewConfiguration(useToCreateLookupViewConfiguration),

                new EditViewConfiguration(elementType:htmlElementTypeInCreationAndEdit),

                new DetailsViewConfiguration()
            };

            return SetConfigurationsViaPropertyInfo(propertyInfo: propertyInfo,
                                                    configurations: configurations,
                                                    displayName: displayName,
                                                    navigation: navigation);
        }


        public ViewDTO SetConfigurationsViaBooleanPropertyInfo(PropertyInfo? propertyInfo,
                                                string? displayName = null,
                                                NavigationColumnInfo? navigation = null,
                                                object? defaultValueInCreation = null,
                                                bool useToCreateCollectionViewConfiguration = true,
                                                bool useToCreateLookupViewConfiguration = true)
        {
            var configurations = new List<IConfiguration>()
            {
                new CreateViewConfiguration(elementType:HtmlElementType.DropdownList,
                                                defaultValue:defaultValueInCreation),

                new ListViewConfiguration(toFilter:new AttributeToFilter(typeName:nameof(EqualFilter)),
                                        toOrder:new AttributeToOrder(isOrderable:true),
                                        elementType:HtmlElementType.DropdownList)
                    .AlsoUseToCreateCollectionViewConfiguration(useToCreateCollectionViewConfiguration:useToCreateCollectionViewConfiguration)
                    .AlsoUseToCreateLookupViewConfiguration(useToCreateLookupViewConfiguration:useToCreateLookupViewConfiguration),

                new EditViewConfiguration(elementType:HtmlElementType.DropdownList),

                new DetailsViewConfiguration(),
            };

            return SetConfigurationsViaPropertyInfo(propertyInfo: propertyInfo,
                                                    configurations: configurations,
                                                    displayName: displayName,
                                                    navigation: navigation);
        }

        /// <summary>
        /// İki tablo arasındaki navigation (ilişki) konfigürasyonunu tip-güvenli lambda ifadeleri ile kurar.
        /// </summary>
        public ViewDTO SetConfigurationsForNavigations<TTargetEntity>(
            Expression<Func<ViewDTO, object>> foreignKeySelector,
            Expression<Func<TTargetEntity, object>> targetDisplayColumnSelector,
            string? displayName = null,
            string? rightTableAlias = null,
            bool setAsLink = true) where TTargetEntity : class
        {
            var foreignKeyName = ReflectionHelper.GetMemberName(foreignKeySelector);
            var targetTableName = typeof(TTargetEntity).Name;
            var displayColumnName = ReflectionHelper.GetMemberName(targetDisplayColumnSelector);

            return SetConfigurationsForNavigations(
                leftTableKeyName: foreignKeyName,
                rightTableName: targetTableName,
                columnNameToDisplay: displayColumnName,
                displayName: displayName ?? displayColumnName,
                rightTableAlias: rightTableAlias,
                setAsLink: setAsLink
            );
        }

        /// <summary>
        /// Hem kaynak DTO'nun hem hedef Entity'nin tip-güvenli olduğu tam generic navigation overload'ı.
        /// </summary>
        public ViewDTO SetConfigurationsForNavigations<TSourceDto, TTargetEntity>(
            Expression<Func<TSourceDto, object>> foreignKeySelector,
            Expression<Func<TTargetEntity, object>> targetDisplayColumnSelector,
            string? displayName = null,
            string? rightTableAlias = null,
            bool setAsLink = true)
            where TSourceDto : class
            where TTargetEntity : class
        {
            var foreignKeyName = ReflectionHelper.GetMemberName(foreignKeySelector);
            var targetTableName = typeof(TTargetEntity).Name;
            var displayColumnName = ReflectionHelper.GetMemberName(targetDisplayColumnSelector);

            return SetConfigurationsForNavigations(
                leftTableKeyName: foreignKeyName,
                rightTableName: targetTableName,
                columnNameToDisplay: displayColumnName,
                displayName: displayName ?? displayColumnName,
                rightTableAlias: rightTableAlias,
                setAsLink: setAsLink
            );
        }

        /// <param name="rightTableAlias">
        /// Aynı tabloya birden fazla join için ayrıştırıcı alias (ör. Request→Employee için
        /// "CreatedByEmployee" ve "OwnerEmployee"). Flatten property önekleri ve FK property adı
        /// bu alias ile aranır: "{alias}Id", "{alias}_{display}", "{alias}_Id".
        /// Null ise rightTableName kullanılır.
        /// </param>
        public ViewDTO SetConfigurationsForNavigations(string leftTableKeyName,
                                                        string rightTableName,
                                                        string rightTableKeyName = "Id",
                                                        string? displayName = null,
                                                        string? columnNameToDisplay = null,
                                                        bool setAsLink = true,
                                                        string? leftTableName = null,
                                                        string? rightTableAlias = null)
        {
            columnNameToDisplay = columnNameToDisplay ?? nameof(IHasId.Id);

            displayName = displayName ?? columnNameToDisplay;

            leftTableName = string.IsNullOrEmpty(leftTableName) ? ControllerName! : leftTableName;

            var aliasPrefix = string.IsNullOrEmpty(rightTableAlias) ? rightTableName : rightTableAlias;

            var foreignKeyPropertyInfo = ReflectionHelper.GetPropertyOf(type: GetType(),
                                                            propertyName: $"{aliasPrefix}Id");

            var selectedPropertyInfo = ReflectionHelper.GetPropertyOf(type: GetType(),
                                                            propertyName: $"{aliasPrefix}_{columnNameToDisplay}");

            PropertyInfo? idPropertyInfo = null;

            var isSelectingIdField = selectedPropertyInfo?.Name == $"{aliasPrefix}_Id";

            if (isSelectingIdField)
                idPropertyInfo = selectedPropertyInfo;
            else
                idPropertyInfo = ReflectionHelper.GetPropertyOf(type: GetType(),
                                                            propertyName: $"{aliasPrefix}_Id");


            //if (foreignKeyPropertyInfo == null)
            //{
            //    throw new Exception($"The property({aliasPrefix}Id) has not found inside {GetType().Name}.cs");
            //}

            if (selectedPropertyInfo == null)
            {
                throw new Exception($"The property({aliasPrefix}_{columnNameToDisplay}) has not found inside {GetType().Name}.cs");
            }

            if (idPropertyInfo == null)
            {
                throw new Exception($"The property({aliasPrefix}_Id) has not found inside {GetType().Name}.cs");
            }

            if (foreignKeyPropertyInfo != null)
                SetConfigurationsViaPropertyInfo(propertyInfo: foreignKeyPropertyInfo,
                   configurations: new List<IConfiguration>()
                   {
                        CreateViewConfiguration.GetForNavigation(),

                        EditViewConfiguration.GetForNavigation()
                   },
                   displayName: displayName,
                   navigation: new NavigationColumnInfo(leftTableName: leftTableName,
                                                        leftTableKeyName: leftTableKeyName,
                                                        rightTableKeyName: rightTableKeyName,
                                                        rightTableName: rightTableName,
                                                        columnNameToDisplay: columnNameToDisplay,
                                                        rightTableAlias: rightTableAlias));

            SetConfigurationsViaPropertyInfo(propertyInfo: selectedPropertyInfo,
               configurations: new List<IConfiguration>()
               {
                    ListViewConfiguration.GetForNavigation(toOrder:new AttributeToOrder(isOrderable:true))
                    .AlsoUseToCreateCollectionViewConfiguration(),

                    DetailsViewConfiguration.GetForNavigation()
                },
               displayName: displayName,
               navigation: new NavigationColumnInfo(leftTableName: leftTableName,
                                                    leftTableKeyName: leftTableKeyName,
                                                    rightTableKeyName: rightTableKeyName,
                                                    rightTableName: rightTableName,
                                                    columnNameToDisplay: columnNameToDisplay,
                                                    setAsLink: setAsLink,
                                                    rightTableAlias: rightTableAlias));


            SetConfigurationsViaPropertyInfo(propertyInfo: idPropertyInfo,
               configurations: new List<IConfiguration>()
               {
                    ListViewConfiguration.GetForNavigation()
                    .AlsoUseToCreateCollectionViewConfiguration(),

                    DetailsViewConfiguration.GetForNavigation()
                },
               displayName: displayName,
               navigation: new NavigationColumnInfo(leftTableName: leftTableName,
                                                    leftTableKeyName: leftTableKeyName,
                                                    rightTableKeyName: rightTableKeyName,
                                                    rightTableName: rightTableName,
                                                    columnNameToDisplay: nameof(IHasId.Id),
                                                    rightTableAlias: rightTableAlias));

            return this;

        }

        public ViewDTO SortByWrittenOrder(ViewType? viewType = null)
        {
            foreach (var configurationGroup in Configurations.GroupBy(c => c.ViewType))
            {
                var configurations = configurationGroup.ToList();

                var counterForSelection = 0;
                var counterForFiltering = 0;

                foreach (var configuration in configurations)
                {
                    if (configuration.ToSelect != null)
                    {
                        configuration.ToSelect.SetPripority(counterForSelection++);
                    }

                    if (configuration.ToFilter != null)
                    {
                        configuration.ToFilter.SetPripority(counterForFiltering++);
                    }
                }
            }

            return this;
        }


        public virtual DTO LoadConfigurations()
        {
            return this;
        }

        public virtual DTO LoadCollections()
        {
            Collections.ForEach(c => c.SetLeftTableName(ControllerName));

            foreach (var item in Collections.Where(c => !c.IsCrossTable))
            {
                item.SetLeftTableName(ControllerName);

                item.SetLeftTableKey("Id");

                item.SetLeftTableKeyValue(Id.ToString());
            }

            return this;
        }

        public List<IConfiguration> GetConfigurations<T>(Expression<Func<T, object>> expression) where T : class
        {
            var memberName = ReflectionHelper.GetMemberName(expression);

            return Configurations.Where(c => c.PropertyName == memberName).ToList();
        }

        public IConfiguration? GetConfiguration<T>(Expression<Func<T, object>> expression, ViewType viewType) where T : class
        {
            var memberName = ReflectionHelper.GetMemberName(expression);

            return Configurations.FirstOrDefault(c => c.PropertyName == memberName && c.ViewType == viewType);
        }

        public virtual ViewDTO RemoveConfigurations<T>(params Expression<Func<T, object>>[] propertyExpressions) where T : class
        {
            if (propertyExpressions == null || propertyExpressions.Length == 0)
                return this;

            var memberNames = new HashSet<string>(propertyExpressions.Select(e => ReflectionHelper.GetMemberName(e)));
            Configurations.RemoveAll(c => memberNames.Contains(c.PropertyName));
            return this;
        }

        public virtual ViewDTO SetConfigurationsForBaseObjectMembers(params Expression<Func<ViewDTO, object>>[] excludeProperties)
        {
            return SetConfigurationsForBaseObjectMembers(inheritingTableName: null, excludeProperties);
        }

        public virtual ViewDTO SetConfigurationsForBaseObjectMembers(string? inheritingTableName = null, params Expression<Func<ViewDTO, object>>[]? excludeProperties)
        {
            bool isInherited = !string.IsNullOrEmpty(inheritingTableName);

            var excludedMemberNames = excludeProperties != null && excludeProperties.Length > 0
                ? new HashSet<string>(excludeProperties.Select(e => ReflectionHelper.GetMemberName(e)))
                : new HashSet<string>();

            if (!excludedMemberNames.Contains(nameof(Description)))
            {
                SetConfigurationsViaPropertyInfo(propertyInfo: ReflectionHelper.GetPropertyOf<ViewDTO>(o => o.Description),
                   configurations: new List<IConfiguration>()
                   {
                       new EditViewConfiguration(elementType:HtmlElementType.TextArea),

                       new DetailsViewConfiguration(),

                   }, displayName: "Description");
            }

            if (!excludedMemberNames.Contains(nameof(AddedDate)))
            {
                SetConfigurationsViaPropertyInfo(propertyInfo: ReflectionHelper.GetPropertyOf<ViewDTO>(o => o.AddedDate),
                   configurations: new List<IConfiguration>()
                   {
                       new ListViewConfiguration(toFilter:new AttributeToFilter(nameof(BetweenFilter)),
                                                    toOrder:new AttributeToOrder(isOrderable:true))
                       .AlsoUseToCreateCollectionViewConfiguration(),

                       new DetailsViewConfiguration()
                   },
                   displayName: "Added Date");
            }

            if (!excludedMemberNames.Contains(nameof(ModifiedDate)))
            {
                SetConfigurationsViaPropertyInfo(propertyInfo: ReflectionHelper.GetPropertyOf<ViewDTO>(o => o.ModifiedDate),
                   configurations: new List<IConfiguration>()
                   {
                       new ListViewConfiguration(toFilter:new AttributeToFilter(nameof(BetweenFilter)),
                                                    toOrder:new AttributeToOrder(isOrderable:true)),

                       new DetailsViewConfiguration(),

                   }, displayName: "Modified Date");
            }

            if (!excludedMemberNames.Contains(nameof(IsActive)))
            {
                SetConfigurationsViaPropertyInfo(propertyInfo: ReflectionHelper.GetPropertyOf<ViewDTO>(o => o.IsActive),
                   configurations: new List<IConfiguration>()
                   {
                        new ListViewConfiguration(toFilter:new AttributeToFilter(nameof(EqualFilter)),
                                                    toOrder:new AttributeToOrder(isOrderable:true),
                                                    elementType:HtmlElementType.DropdownList)
                                                    .AlsoUseToCreateCollectionViewConfiguration(),

                        new EditViewConfiguration(),

                        new DetailsViewConfiguration(),

                   }, displayName: "Active");
            }

            //NOT: IsHidden sadece ViewDTO üzerinde yaşayan bir UI alanıdır; BaseObject entity'lerinde
            //karşılığı (DB kolonu) yoktur. Konfigüre edilirse SELECT'e girer ve
            //"Invalid column name 'IsHidden'" SQL hatası üretir. Bu yüzden burada konfigüre edilmez.

            return this;
        }
    }

}
