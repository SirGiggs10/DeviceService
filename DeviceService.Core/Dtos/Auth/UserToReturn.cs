using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeviceService.Core.Dtos.Auth
{
    public class UserToReturn
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public bool Deleted { get; set; }
        public string FullName { get; set; }
        public string Address { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public int UserType { get; set; }
        public int UserTypeId { get; set; }
        public bool EmailConfirmed { get; set; }
        public DateTimeOffset? LastLoginDateTime { get; set; }
        public DateTimeOffset? SecondToLastLoginDateTime { get; set; }
    }
}
