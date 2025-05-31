using Hydra.DataModels.Filter;
using Hydra.DataModels;
using Hydra.DTOs.ViewDTOs;
using Hydra.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.DTOs.ViewConfigurations
{
    public class ListViewConfiguration : Configuration
    {
        public bool ToCreateCollectionViewConfiguration { get; private set; }

        public bool ToCreateLookupViewConfiguration { get; private set; }

        public bool ThenRemoveThis { get; set; }

        //public ListViewConfiguration() { }

        public ListViewConfiguration(
             AttributeToSelect? toSelect,
             AttributeToFilter? toFilter = null,
             AttributeToOrder? toOrder = null,
             NavigationColumnInfo? navigation = null,
             HtmlElementType elementType = HtmlElementType.Input,
             HtmlInputType inputType = HtmlInputType.text,
             string? displayName = null,
             object? defaultValue = null,
             bool setToCreateResultViewFromThis = true)
             : base(viewType: ViewType.ListView,
                   viewName: ViewType.ListView.ToString(),
                   toSelect: toSelect,
                   toFilter: toFilter,
                   toOrder: toOrder,
                   navigation: navigation,
                   elementType: elementType,
                   inputType: inputType,
                   displayName: displayName,
                   defaultValue: defaultValue,
                   setToCreateResultViewFromThis: setToCreateResultViewFromThis)
        {

        }

        public ListViewConfiguration(
                int priority = 0,
                AttributeToFilter? toFilter = null,
                AttributeToOrder? toOrder = null,
                NavigationColumnInfo? navigation = null,
                HtmlElementType elementType = HtmlElementType.Input,
                HtmlInputType inputType = HtmlInputType.text,
                string? displayName = null,
                object? defaultValue = null,
                bool setToCreateResultViewFromThis = true)
                : base(viewType: ViewType.ListView,
                      viewName: ViewType.ListView.ToString(),
                      toSelect: new AttributeToSelect(priority),
                      toFilter: toFilter,
                      toOrder: toOrder,
                      navigation: navigation,
                      elementType: elementType,
                      inputType: inputType,
                      displayName: displayName,
                      defaultValue: defaultValue,
                      setToCreateResultViewFromThis: setToCreateResultViewFromThis)
        {

        }

        public static ListViewConfiguration GetForNavigation(
            int selectPriority = 0,
             int filterPriority = 0,
             bool isDisabled = false,
             AttributeToOrder? toOrder = null,
             AttributeToFilter? toFilter = null,
             NavigationColumnInfo? navigation = null,
            string? displayName = null,
            object? defaultValue = null,
            bool setToCreateResultViewFromThis = true)
        {
            return new ListViewConfiguration(toSelect: new AttributeToSelect(selectPriority),
                toFilter: toFilter ?? new AttributeToFilter(typeName: nameof(EqualFilter), priority: filterPriority, isDisabled: isDisabled),
                toOrder: toOrder,
                navigation: navigation,
                elementType: HtmlElementType.DataList,
                displayName: displayName,
                defaultValue: defaultValue,
                setToCreateResultViewFromThis: setToCreateResultViewFromThis);
        }

        //public static ListViewConfiguration GetForNavigation(int selectPriority = 0,
        //     int filterPriority = 0,
        //     AttributeToOrder toOrder = null,
        //     NavigationColumnInfo navigation = null,
        //    string displayName = null,
        //    object defaultValue = null)
        //        {
        //            return GetForNavigation(selectPriority: selectPriority,
        //                filterPriority: filterPriority,
        //                toOrder: toOrder,
        //                navigation:navigation,
        //                displayName: displayName,
        //                defaultValue: defaultValue);
        //        }

        public IConfiguration ForCollectionView()
        {
            return SetViewType(ViewType.CollectionView);
        }

        public IConfiguration ForLookupView()
        {
            return SetViewType(ViewType.LookupView);
        }

        public ListViewConfiguration AlsoUseToCreateCollectionViewConfiguration(bool useToCreateCollectionViewConfiguration = true, bool thenRemoveThis = false)
        {
            ToCreateCollectionViewConfiguration = useToCreateCollectionViewConfiguration;

            ThenRemoveThis = thenRemoveThis;

            return this;
        }

        public ListViewConfiguration AlsoUseToCreateLookupViewConfiguration(bool useToCreateLookupViewConfiguration = true, bool thenRemoveThis = false)
        {
            ToCreateLookupViewConfiguration = useToCreateLookupViewConfiguration;

            ThenRemoveThis = thenRemoveThis;

            return this;
        }

        //public CollectionViewConfiguration ConvertToAnotherViewConfiguration(ViewType viewType)
        //{
        //    var anotherViewConfiguration = ReflectionHelper.Clone<CollectionViewConfiguration>(this);

        //    anotherViewConfiguration.SetPropertyInfo(PropertyInfo);

        //    if (ToSelect != null)
        //        anotherViewConfiguration.ToSelect = ReflectionHelper.Clone<AttributeToSelect>(ToSelect);

        //    if (ToFilter != null)
        //        anotherViewConfiguration.ToFilter = ReflectionHelper.Clone<AttributeToFilter>(ToFilter);

        //    if (ToOrder != null)
        //        anotherViewConfiguration.ToOrder = ReflectionHelper.Clone<AttributeToOrder>(ToOrder);

        //    if (Navigation != null)
        //        anotherViewConfiguration.Navigation = ReflectionHelper.Clone<NavigationColumnInfo>(Navigation);

        //    anotherViewConfiguration.SetViewType(viewType);

        //    return anotherViewConfiguration;
        //}

        public Configuration ConvertToAnotherViewConfiguration(ViewType viewType)
        {
            Configuration anotherViewConfiguration;

            switch (viewType)
            {
                case ViewType.CollectionView:
                    anotherViewConfiguration = ReflectionHelper.Clone<CollectionViewConfiguration>(this);
                    break;

                case ViewType.LookupView:
                    anotherViewConfiguration = ReflectionHelper.Clone<LookupViewConfiguration>(this);
                    break;

                default:
                    throw new NotSupportedException($"'{viewType}' tipi desteklenmiyor.");
            }

            anotherViewConfiguration.SetPropertyInfo(PropertyInfo);

            if (ToSelect != null)
                anotherViewConfiguration.ToSelect = ReflectionHelper.Clone<AttributeToSelect>(ToSelect);

            if (ToFilter != null)
                anotherViewConfiguration.ToFilter = ReflectionHelper.Clone<AttributeToFilter>(ToFilter);

            if (ToOrder != null)
                anotherViewConfiguration.ToOrder = ReflectionHelper.Clone<AttributeToOrder>(ToOrder);

            if (Navigation != null)
                anotherViewConfiguration.Navigation = ReflectionHelper.Clone<NavigationColumnInfo>(Navigation);

            anotherViewConfiguration.SetViewType(viewType);

            return anotherViewConfiguration;
        }



        public new ListViewConfiguration SetPropertyInfo(PropertyInfo? propertyInfo)
        {
            return (ListViewConfiguration)base.SetPropertyInfo(propertyInfo);
        }

        public new ListViewConfiguration SetTableName(string? tableName)
        {
            return (ListViewConfiguration)base.SetTableName(tableName);
        }
    }
}
