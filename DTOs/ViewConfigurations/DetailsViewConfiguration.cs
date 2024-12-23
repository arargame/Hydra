using Hydra.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hydra.DTOs.ViewDTOs;

namespace Hydra.DTOs.ViewConfigurations
{
    public class DetailsViewConfiguration : Configuration
    {
        public DetailsViewConfiguration(
            AttributeToSelect toSelect,
            NavigationColumnInfo navigation = null,
            HtmlElementType elementType = HtmlElementType.Input,
            HtmlInputType inputType = HtmlInputType.text,
            string displayName = null,
            object defaultValue = null,
            bool setToCreateResultViewFromThis = true)
            : base(viewType: ViewType.DetailsView,
                  viewName: ViewType.DetailsView.ToString(),
                  toSelect: toSelect,
                  toFilter: null,
                  toOrder: null,
                  navigation: navigation,
                  elementType: elementType,
                  inputType: inputType,
                  displayName: displayName,
                  defaultValue: defaultValue,
                  setToCreateResultViewFromThis: setToCreateResultViewFromThis)
        {

        }

        public DetailsViewConfiguration(
            int priority = 0,
            NavigationColumnInfo navigation = null,
            HtmlElementType elementType = HtmlElementType.Input,
            HtmlInputType inputType = HtmlInputType.text,
            string displayName = null,
            object defaultValue = null,
            bool setToCreateResultViewFromThis = true)
            : base(viewType: ViewType.DetailsView,
                  viewName: ViewType.DetailsView.ToString(),
                  toSelect: new AttributeToSelect(priority),
                  toFilter: null,
                  toOrder: null,
                  navigation: navigation,
                  elementType: elementType,
                  inputType: inputType,
                  displayName: displayName,
                  defaultValue: defaultValue,
                  setToCreateResultViewFromThis: setToCreateResultViewFromThis)
        {

        }


        public static DetailsViewConfiguration GetForNavigation(
            int selectPriority = 0,
            NavigationColumnInfo navigation = null,
            string displayName = null,
            object defaultValue = null,
            bool setToCreateResultViewFromThis = true)
        {
            return new DetailsViewConfiguration(toSelect: new AttributeToSelect(selectPriority),
                navigation: navigation,
                displayName: displayName,
                defaultValue: defaultValue,
                setToCreateResultViewFromThis: setToCreateResultViewFromThis);
        }
    }
}
