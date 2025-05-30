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


        public ViewDTO SetConfigurationsViaEnumPropertyInfo(PropertyInfo propertyInfo,
                                                string? displayName = null,
                                                NavigationColumnInfo? navigation = null,
                                                object? defaultValue = null)
        {
            var configurations = new List<IConfiguration>()
            {
                   new CreateViewConfiguration(elementType: HtmlElementType.DropdownList,defaultValue:defaultValue),

                   new ListViewConfiguration(toFilter: new AttributeToFilter(priority: 0, typeName: nameof(EqualFilter)),
                                            toOrder: new AttributeToOrder(isOrderable: true),
                                            elementType: HtmlElementType.DropdownList),

                   new EditViewConfiguration(elementType: HtmlElementType.DropdownList),

                   new DetailsViewConfiguration(),
            };

            return SetConfigurationsViaPropertyInfo(propertyInfo: propertyInfo,
                                                    configurations: configurations,
                                                    displayName: displayName,
                                                    navigation: navigation);
        }

        public ViewDTO SetConfigurationsViaStringPropertyInfo(PropertyInfo propertyInfo,
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


        public ViewDTO SetConfigurationsViaBooleanPropertyInfo(PropertyInfo propertyInfo,
                                                string displayName = null,
                                                NavigationColumnInfo navigation = null,
                                                object defaultValueInCreation = null,
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

        public ViewDTO SetConfigurationsForNavigations(string leftTableKeyName,
                                                        string rightTableName,
                                                        string rightTableKeyName = "Id",
                                                        string? displayName = null,
                                                        string? columnNameToDisplay = null,
                                                        bool setAsLink = true,
                                                        string? leftTableName = null)
        {
            columnNameToDisplay = columnNameToDisplay ?? nameof(IHasId.Id);

            displayName = displayName ?? columnNameToDisplay;

            leftTableName = string.IsNullOrEmpty(leftTableName) ? ControllerName! : leftTableName;

            var foreignKeyPropertyInfo = ReflectionHelper.GetPropertyOf(type: GetType(),
                                                            propertyName: $"{rightTableName}Id");

            var selectedPropertyInfo = ReflectionHelper.GetPropertyOf(type: GetType(),
                                                            propertyName: $"{rightTableName}_{columnNameToDisplay}");

            PropertyInfo? idPropertyInfo = null;

            var isSelectingIdField = selectedPropertyInfo?.Name == $"{rightTableName}_Id";

            if (isSelectingIdField)
                idPropertyInfo = selectedPropertyInfo;
            else
                idPropertyInfo = ReflectionHelper.GetPropertyOf(type: GetType(),
                                                            propertyName: $"{rightTableName}_Id");


            //if (foreignKeyPropertyInfo == null)
            //{
            //    throw new Exception($"The property({rightTableName}Id) has not found inside {GetType().Name}.cs");
            //}

            if (selectedPropertyInfo == null)
            {
                throw new Exception($"The property({rightTableName}_{columnNameToDisplay}) has not found inside {GetType().Name}.cs");
            }

            if (idPropertyInfo == null)
            {
                throw new Exception($"The property({rightTableName}_Id) has not found inside {GetType().Name}.cs");
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
                                                        columnNameToDisplay: columnNameToDisplay));

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
                                                    setAsLink: setAsLink));


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
                                                    columnNameToDisplay: nameof(IHasId.Id)));

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

        public IConfiguration GetConfiguration<T>(Expression<Func<T, object>> expression, ViewType viewType) where T : class
        {
            var memberName = ReflectionHelper.GetMemberName(expression);

            return Configurations.FirstOrDefault(c => c.PropertyName == memberName && c.ViewType == viewType);
        }

        public virtual ViewDTO SetConfigurationsForBaseObjectMembers(string inheritingTableName = null)
        {
            bool isInherited = !string.IsNullOrEmpty(inheritingTableName);

            SetConfigurationsViaPropertyInfo(propertyInfo: ReflectionHelper.GetPropertyOf<ViewDTO>(o => o.Description),
               configurations: new List<IConfiguration>()
               {
                   new EditViewConfiguration(elementType:HtmlElementType.TextArea),

                   new DetailsViewConfiguration(),

               }, displayName: "Description");


            SetConfigurationsViaPropertyInfo(propertyInfo: ReflectionHelper.GetPropertyOf<ViewDTO>(o => o.AddedDate),
               configurations: new List<IConfiguration>()
               {
                   new ListViewConfiguration(toFilter:new AttributeToFilter(nameof(BetweenFilter)),
                                                toOrder:new AttributeToOrder(isOrderable:true))
                   .AlsoUseToCreateCollectionViewConfiguration(),

                   new DetailsViewConfiguration()
               },
               displayName: "Added Date");

            SetConfigurationsViaPropertyInfo(propertyInfo: ReflectionHelper.GetPropertyOf<ViewDTO>(o => o.ModifiedDate),
               configurations: new List<IConfiguration>()
               {
                   new ListViewConfiguration(toFilter:new AttributeToFilter(nameof(BetweenFilter)),
                                                toOrder:new AttributeToOrder(isOrderable:true)),

                   new DetailsViewConfiguration(),

               }, displayName: "Modified Date");

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

            SetConfigurationsViaPropertyInfo(propertyInfo: ReflectionHelper.GetPropertyOf<ViewDTO>(o => o.IsHidden),
               configurations: new List<IConfiguration>()
               {
                   new ListViewConfiguration(toFilter:new AttributeToFilter(nameof(EqualFilter)),
                                            toOrder:new AttributeToOrder(isOrderable:true),
                                            elementType:HtmlElementType.DropdownList),

                   new EditViewConfiguration(),

                   new DetailsViewConfiguration(),

               }, displayName: "Hidden");

            return this;
        }
    }

}
