using Ayuda_Help_Desk.Dtos.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ayuda_Help_Desk.Dtos.RoleFunctionality
{
    public class UserRoleResponse
    {
        public int Id { get; set; }
        public UserToReturn User { get; set; }
        public RoleResponse Role { get; set; }
    }
}
