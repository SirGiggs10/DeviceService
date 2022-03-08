using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DeviceService.Core.Entities
{
    public class AuditReport
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int AuditReportId { get; set; }
        public int AuditReportActivityId { get; set; }
        public int UserId { get; set; }
        public string AuditReportActivityResourceId { get; set; }   //STRINGIFIED LIST OF INT
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;

        public virtual AuditReportActivity AuditReportActivity { get; set; }
        public virtual User User { get; set; }
    }
}
