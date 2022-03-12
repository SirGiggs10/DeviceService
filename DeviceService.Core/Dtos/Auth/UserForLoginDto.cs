using DeviceService.Core.Helpers.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeviceService.Core.Dtos.Auth
{
    public class UserForLoginDto
    {
        [Encrypted]
        public string EmailAddress { get; set; }
        [Encrypted]
        public string Password { get; set; }
    }
}
