using DeviceService.Core.Helpers.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeviceService.Core.Dtos.Auth
{
    public class ChangePasswordRequest
    {
        [Encrypted]
        public string OldPassword { get; set; }
        [Encrypted]
        public string NewPassword { get; set; }
    }
}
