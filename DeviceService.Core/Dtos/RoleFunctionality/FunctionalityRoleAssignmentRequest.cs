using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ayuda_Help_Desk.Dtos.RoleFunctionality
{
    public class FunctionalityRoleAssignmentRequest
    {
        public RoleRequest RoleRequest { get; set; }
        public List<FunctionalityResponse> Functionalities { get; set; }
    }
}