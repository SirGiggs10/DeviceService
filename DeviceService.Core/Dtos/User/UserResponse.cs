using DeviceService.Core.Dtos.RoleFunctionality;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceService.Core.Dtos.User
{
    public class UserResponse
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public int UserType { get; set; }
        public int UserTypeId { get; set; }
        public bool EmailConfirmed { get; set; }
        public DateTimeOffset? LastLoginDateTime { get; set; }
        public DateTimeOffset? SecondToLastLoginDateTime { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public bool Deleted { get; set; }
        public List<UserRoleResponse> UserRoles { get; set; }
    }
}
