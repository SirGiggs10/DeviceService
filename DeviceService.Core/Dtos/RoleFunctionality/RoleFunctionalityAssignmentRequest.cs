using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ayuda_Help_Desk.Dtos.RoleFunctionality
{
    public class RoleFunctionalityAssignmentRequest
    {
        public FunctionalityResponse Functionality { get; set; }
        public List<RoleResponse> Roles { get; set; }
    }
}