using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeviceService.Core.Dtos.Auth
{
    public class UserLoginResponseForLogin
    {
        public string Token { get; set; }
        public UserToReturnForLogin User { get; set; }
    }
}
