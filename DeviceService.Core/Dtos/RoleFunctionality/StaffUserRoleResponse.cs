using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ayuda_Help_Desk.Dtos.RoleFunctionality
{
    public class StaffUserRoleResponse
    {
        public int Id { get; set; }
        public StaffUserToReturn User { get; set; }
        public RoleResponse Role { get; set; }
    }
}
