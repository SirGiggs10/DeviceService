using Ayuda_Help_Desk.Dtos.Department;
using Ayuda_Help_Desk.Dtos.SubUnit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ayuda_Help_Desk.Dtos.Staff
{
    public class StaffResponse
    {
        public int StaffId { get; set; }
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
        public string StaffProfilePictureUrl { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public SupervisorResponse Supervisor { get; set; }
        public StaffDepartmentResponse Department { get; set; }
        public StaffSubUnitResponse SubUnit { get; set; }
        public BranchResponse Branch { get; set; }
    }
}
