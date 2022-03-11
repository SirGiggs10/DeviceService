using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeviceService.Core.Entities
{
    public class Role : IdentityRole<int>
    {
        public string RoleName { get; set; }
        public string RoleDescription { get; set; }
        public int UserType { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;
        public DateTimeOffset? ModifiedAt { get; set; }

        public virtual List<UserRole> UserRoles { get; set; }
        public virtual List<FunctionalityRole> FunctionalityRoles { get; set; }
    }
}
