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
    public class EditViewConfiguration : Configuration
    {
        public EditViewConfiguration(AttributeToFilter toFilter,
            NavigationColumnInfo? navigation = null,
            HtmlElementType elementType = HtmlElementType.Input,
            HtmlInputType inputType = HtmlInputType.text,
            string? displayName = null,
            object? defaultValue = null,
            bool isForeignKey = false)
            : base(viewType: ViewType.EditView,
                viewName: ViewType.EditView.ToString(),
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

        public EditViewConfiguration(
                int priority = 0,
                bool isDisabled = false,
                NavigationColumnInfo? navigation = null,
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

        public static EditViewConfiguration GetForNavigation(int priority = 0,
            bool isDisabled = false,
            NavigationColumnInfo? navigation = null,
            string? displayName = null,
            object? defaultValue = null)
        {
            return new EditViewConfiguration(toFilter: new AttributeToFilter(typeName: nameof(EqualFilter), priority: priority, isDisabled: isDisabled),
                navigation: navigation,
                elementType: HtmlElementType.DataList,
                inputType: HtmlInputType.text,
                displayName: displayName,
                defaultValue: defaultValue,
                isForeignKey: true);
        }
    }
}

