using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace DeviceService.Core.Entities
{
    public class User : IdentityUser<int>
    {
        // Add additional profile data for application users by adding properties to this class// public int Id { get; set; }
        public string Name { get; set; }
        public int UserType { get; set; } // specifies the type of user
        public int UserTypeId { get; set; } // specifies the id of the user on his type table
        public bool Deleted { get; set; }
        public string ShortToken { get; set; }
        public string LongToken { get; set; }
        public string FullName { get; set; }
        public string Address { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;
        public DateTimeOffset? LastLoginDateTime { get; set; }
        public DateTimeOffset? SecondToLastLoginDateTime { get; set; }

        public virtual List<UserRole> UserRoles { get; set; }
        public virtual List<AuditReport> AuditReports { get; set; }
    }
}
