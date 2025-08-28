using Hydra.AccessManagement;
using Hydra.DTOs.ViewDTOs;
using Hydra.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.DTOs.ModelDTOs.SystemUserDTO
{
    [RegisterAsViewDTO(nameof(SystemUser))]
    public class SystemUserViewDTO :ViewDTO
    {

        public string? Email { get; set; } = null;

        public SystemUserViewDTO()
        {
            SetControllerName("SystemUser");
        }

        public override DTO LoadConfigurations()
        {
            SetConfigurationsForBaseObjectMembers();

            SetConfigurationsViaStringPropertyInfo(propertyInfo:ReflectionHelper.GetPropertyOf(type : GetType(),propertyName:nameof(SystemUserViewDTO.Email)),
                displayName:nameof(SystemUserViewDTO.Email));

            return base.LoadConfigurations();
        }
    }
}
