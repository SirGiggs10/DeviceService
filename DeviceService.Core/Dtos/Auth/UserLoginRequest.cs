using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ayuda_Help_Desk.Dtos.Auth
{
    public class UserLoginRequest
    {
        public string Password { get; set; }
        public string EmailAddress { get; set; }
        public int UserId { get; set; }
    }
}
