using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeviceService.Core.Dtos.RoleFunctionality
{
    public class RoleFunctionalityAssignmentRequest
    {
        public FunctionalityResponse Functionality { get; set; }
        public List<RoleResponse> Roles { get; set; }
    }
}