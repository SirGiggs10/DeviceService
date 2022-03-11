using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeviceService.Core.Dtos.RoleFunctionality
{
    public class FunctionalityRoleAssignmentRequest
    {
        public RoleRequest RoleRequest { get; set; }
        public List<FunctionalityResponse> Functionalities { get; set; }
    }
}