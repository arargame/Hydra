using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.DataModels
{
    public class NavigationColumnInfo
    {
        public Guid? LeftTableKeyValue { get; set; }

        public string? LeftTableName { get; set; }

        public string? LeftTableKeyName { get; set; }

        public string? RightTableKeyName { get; set; }

        public string? RightTableName { get; set; }

        public Guid? RightTableKeyValue { get; set; }

        public string? NameToDisplay { get; set; }

        public bool SetAsLink { get; set; }

        public NavigationColumnInfo() { }
        public NavigationColumnInfo(string leftTableName, string leftTableKeyName, string rightTableKeyName, string rightTableName, string columnNameToDisplay, bool setAsLink = false)
        {
            LeftTableName = leftTableName;

            LeftTableKeyName = leftTableKeyName;

            NameToDisplay = columnNameToDisplay;

            RightTableKeyName = rightTableKeyName;

            RightTableName = rightTableName;

            SetAsLink = setAsLink;
        }
    }
}
