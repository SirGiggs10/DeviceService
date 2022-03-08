using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DeviceService.Core.Entities
{
    public class FunctionalityRole
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public int FunctionalityId { get; set; }
        public string FunctionalityName { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;

        public virtual Functionality Functionality { get; set; }
        public virtual Role Role { get; set; }
    }
}
