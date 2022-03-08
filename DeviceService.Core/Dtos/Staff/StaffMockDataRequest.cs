using Ayuda_Help_Desk.Dtos.RoleFunctionality;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ayuda_Help_Desk.Dtos.Staff
{
    public class StaffMockDataRequest
    {
        public string CompanyId { get; set; }
        public string FullName { get; set; }
        public int DepartmentId { get; set; }
        public int SubUnitId { get; set; }
        public int SupervisorId { get; set; }
        public int BranchId { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public string ResidentialAddress { get; set; }
        public DateTimeOffset? DateOfBirth { get; set; }
        public List<MockRoleRequest> Roles { get; set; }
    }
}
