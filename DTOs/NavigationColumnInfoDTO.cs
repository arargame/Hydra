using Hydra.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.DTOs
{
    public class NavigationColumnInfoDTO
    {
        public Guid? LeftTableKeyValue { get; set; }

        public string LeftTableName { get; set; }

        public string LeftTableKeyName { get; set; }

        public string NameToDisplay { get; set; }

        public string RightTableKeyName { get; set; }

        public string RightTableName { get; set; }

        public Guid? RightTableKeyValue { get; set; }

        public bool SetAsLink { get; set; }

        public NavigationColumnInfoDTO()
        {

        }

        public NavigationColumnInfoDTO(string leftTableName, string leftTableKeyName, string rightTableKeyName, string rightTableName, string nameToDisplay, bool setAsLink = false)
        {
            LeftTableName = leftTableName;

            LeftTableKeyName = leftTableKeyName;

            RightTableKeyName = rightTableKeyName;

            RightTableName = rightTableName;

            NameToDisplay = nameToDisplay;

            SetAsLink = setAsLink;
        }

        public static NavigationColumnInfoDTO? ConvertToNavigationColumnInfoDTO(NavigationColumnInfo? navigationColumnInfo)
        {
            if (navigationColumnInfo == null)
                return null;

            var navigationColumnInfoDTO = new NavigationColumnInfoDTO()
            {
                LeftTableKeyValue = navigationColumnInfo.LeftTableKeyValue,
                LeftTableName = navigationColumnInfo.LeftTableName,
                LeftTableKeyName = navigationColumnInfo.LeftTableKeyName,
                RightTableKeyName = navigationColumnInfo.RightTableKeyName,
                RightTableName = navigationColumnInfo.RightTableName,
                RightTableKeyValue = navigationColumnInfo.RightTableKeyValue,
                NameToDisplay = navigationColumnInfo.NameToDisplay,
                SetAsLink = navigationColumnInfo.SetAsLink
            };

            return navigationColumnInfoDTO;
        }

        public static NavigationColumnInfo ConvertToNavigationColumnInfo(NavigationColumnInfoDTO navigationColumnInfoDTO)
        {
            if (navigationColumnInfoDTO == null)
                return null;

            var navigationColumnInfo = new NavigationColumnInfo(leftTableName: navigationColumnInfoDTO.LeftTableName,
                leftTableKeyName: navigationColumnInfoDTO.LeftTableKeyName,
                rightTableKeyName: navigationColumnInfoDTO.RightTableKeyName,
                rightTableName: navigationColumnInfoDTO.RightTableName,
                columnNameToDisplay: navigationColumnInfoDTO.NameToDisplay,
                setAsLink: navigationColumnInfoDTO.SetAsLink)
            {
                LeftTableKeyValue = navigationColumnInfoDTO.LeftTableKeyValue,
                RightTableKeyValue = navigationColumnInfoDTO.RightTableKeyValue
            };

            return navigationColumnInfo;
        }
    }
}
