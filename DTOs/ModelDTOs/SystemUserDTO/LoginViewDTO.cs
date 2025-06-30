using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.DTOs.ModelDTOs.SystemUserDTO
{
    public class LoginViewDTO
    {
        public Guid Id { get; set; }

        public string? UserName { get; set; }

        public string? EmailAddress { get; set; }

        public string? UserNameOrEmailAddress { get; set; }

        public string? Password { get; set; }

        public string? Token { get; set; }

        public string? LocalStorageKeyName { get; set; }

        public string? JwtToken { get; set; }

        public string? ReturnUrl { get; set; }
    }
}
