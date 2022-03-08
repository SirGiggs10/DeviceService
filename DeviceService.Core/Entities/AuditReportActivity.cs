using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Ayuda_Help_Desk.Models
{
    public class AuditReportActivity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int AuditReportActivityId { get; set; }
        //public string AuditReportActivityName { get; set; }
        public int FunctionalityId { get; set; }
        public string AuditReportActivityDescription { get; set; }
        public string AuditReportActivityViewUrl { get; set; }
        public string FrontendRoute { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }

        public virtual List<AuditReport> AuditReports { get; set; }
        public virtual Functionality Functionality { get; set; }
    }
}
