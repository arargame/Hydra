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

        /// <summary>
        /// Aynı tabloya birden fazla join yapılacaksa (ör. Request → Employee hem CreatedBy hem Owner)
        /// join'i ve flatten edilen property önekini ayrıştıran alias.
        /// Null ise RightTableName kullanılır.
        /// </summary>
        public string? RightTableAlias { get; set; }

        public string EffectiveRightTableAlias => string.IsNullOrEmpty(RightTableAlias) ? RightTableName ?? string.Empty : RightTableAlias!;

        public Guid? RightTableKeyValue { get; set; }

        public string? NameToDisplay { get; set; }

        public bool SetAsLink { get; set; }

        public NavigationColumnInfo() { }
        public NavigationColumnInfo(string leftTableName, string leftTableKeyName, string rightTableKeyName, string rightTableName, string columnNameToDisplay, bool setAsLink = false, string? rightTableAlias = null)
        {
            LeftTableName = leftTableName;

            LeftTableKeyName = leftTableKeyName;

            NameToDisplay = columnNameToDisplay;

            RightTableKeyName = rightTableKeyName;

            RightTableName = rightTableName;

            RightTableAlias = rightTableAlias;

            SetAsLink = setAsLink;
        }
    }
}
