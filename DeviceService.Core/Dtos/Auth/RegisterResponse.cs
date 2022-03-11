using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeviceService.Core.Dtos.Auth
{
    public class RegisterResponse
    {
        public UserLoginRequest UserLoginDetails { get; set; }
        public object ObjectValue { get; set; }
    }
}
