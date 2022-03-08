using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DeviceService.Core.Entities
{
    public class Functionality
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int FunctionalityId { get; set; }
        public string FunctionalityName { get; set; }
        public string FunctionalityDescription { get; set; }
        public int ProjectModuleId { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;

        public virtual ProjectModule ProjectModule { get; set; }
        public virtual List<FunctionalityRole> FunctionalityRoles { get; set; }
        public virtual AuditReportActivity AuditReportActivity { get; set; }
    }
}
