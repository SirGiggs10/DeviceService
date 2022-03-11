using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeviceService.Core.Dtos.RoleFunctionality
{
    public class UserAndRoleResponse
    {
        public int Id { get; set; }
        public int UserType { get; set; } // specifies the type of user like administrator/normal user and other types of user if exists
        public int UserTypeId { get; set; } // specifies the id of the user on his type table...like candidate table
        public List<UserRoleToReturn> UserRoles { get; set; }
    }
}
