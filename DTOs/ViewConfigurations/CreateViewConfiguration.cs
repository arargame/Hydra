using Hydra.DataModels.Filter;
using Hydra.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hydra.DTOs.ViewDTOs;

namespace Hydra.DTOs.ViewConfigurations
{
    public class CreateViewConfiguration : Configuration
    {
        public CreateViewConfiguration(
            AttributeToFilter toFilter,
            NavigationColumnInfo? navigation = null,
            HtmlElementType elementType = HtmlElementType.Input,
            HtmlInputType inputType = HtmlInputType.text,
            string displayName = null,
            object defaultValue = null,
            bool isForeignKey = false)
            : base(viewType: ViewType.CreateView,
                viewName: ViewType.CreateView.ToString(),
                toSelect: null,
                toFilter: toFilter,
                toOrder: null,
                navigation: navigation,
                elementType: elementType,
                inputType: inputType,
                displayName: displayName,
                defaultValue: defaultValue,
                isForeignKey: isForeignKey)
        {

        }

        public CreateViewConfiguration(
                int priority = 0,
                bool isDisabled = false,
                NavigationColumnInfo navigation = null,
                HtmlElementType elementType = HtmlElementType.Input,
                HtmlInputType inputType = HtmlInputType.text,
                string? displayName = null,
                object? defaultValue = null,
                bool isForeignKey = false)
                : this(toFilter: new AttributeToFilter(typeName: nameof(EqualFilter), priority: priority, isDisabled: isDisabled),
                      navigation: navigation,
                      elementType: elementType,
                      inputType: inputType,
                      displayName: displayName,
                      defaultValue: defaultValue,
                      isForeignKey: isForeignKey)
        {

        }

        public static CreateViewConfiguration GetForNavigation(
            NavigationColumnInfo? navigation = null,
            int selectPriority = 0,
             int filterPriority = 0,
             bool isDisabled = false,
            string? displayName = null,
            object? defaultValue = null,
            bool isForeignKey = true)
        {
            return new CreateViewConfiguration(toFilter: new AttributeToFilter(typeName: nameof(EqualFilter), priority: filterPriority, isDisabled: isDisabled),
                navigation: navigation,
                elementType: HtmlElementType.DataList,
                displayName: displayName,
                defaultValue: defaultValue,
                isForeignKey: isForeignKey);
        }
    }
}
