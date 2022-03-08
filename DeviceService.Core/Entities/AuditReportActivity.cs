using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DeviceService.Core.Entities
{
    public class AuditReportActivity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int AuditReportActivityId { get; set; }
        //public string AuditReportActivityName { get; set; }
        public int FunctionalityId { get; set; }
        public string AuditReportActivityDescription { get; set; }
        public string AuditReportActivityViewUrl { get; set; }
        public string FrontendRoute { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;

        public virtual List<AuditReport> AuditReports { get; set; }
        public virtual Functionality Functionality { get; set; }
    }
}
