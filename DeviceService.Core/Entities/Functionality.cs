using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Ayuda_Help_Desk.Models
{
    public class Functionality
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int FunctionalityId { get; set; }
        public string FunctionalityName { get; set; }
        public string FunctionalityDescription { get; set; }
        public int ProjectModuleId { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }

        public virtual ProjectModule ProjectModule { get; set; }
        public virtual List<FunctionalitySupportLevel> FunctionalitySupportLevels { get; set; }
        public virtual AuditReportActivity AuditReportActivity { get; set; }
    }
}
