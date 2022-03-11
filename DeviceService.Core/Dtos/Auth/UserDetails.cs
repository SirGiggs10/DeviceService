using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeviceService.Core.Dtos.Auth
{
    public class UserDetails
    {
        public string Token { get; set; }
        public Entities.User User { get; set; }
    }
}
