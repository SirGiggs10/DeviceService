using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Ayuda_Help_Desk.Models
{
    public class AuditReport
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int AuditReportId { get; set; }
        public int AuditReportActivityId { get; set; }
        public int UserId { get; set; }
        public string AuditReportActivityResourceId { get; set; }   //STRINGIFIED LIST OF INT
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }

        public virtual AuditReportActivity AuditReportActivity { get; set; }
        public virtual User User { get; set; }
    }
}
