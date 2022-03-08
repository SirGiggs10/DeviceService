using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ayuda_Help_Desk.Dtos.RoleFunctionality
{
    public class FunctionalityRoleResponse
    {
        public int Id { get; set; }
        public int RoleId { get; set; }
        public string FunctionalityName { get; set; }
        public string RoleName { get; set; }
    }
}
