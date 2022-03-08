using Ayuda_Help_Desk.Dtos.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ayuda_Help_Desk.Dtos.RoleFunctionality
{
    public class RoleUserAssignmentRequest
    {
        public List<UserToReturn> Users { get; set; }
        public List<RoleResponse> Roles { get; set; }
    }
}