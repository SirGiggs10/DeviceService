using Ayuda_Help_Desk.Dtos.AuditReportActivity;
using Ayuda_Help_Desk.Dtos.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ayuda_Help_Desk.Dtos.AuditReport
{
    public class AuditReportResponse
    {
        public int AuditReportId { get; set; }
        public int AuditReportActivityId { get; set; }
        public int UserId { get; set; }
        public string AuditReportActivityResourceId { get; set; }   //STRINGIFIED LIST OF INT
        public DateTimeOffset CreatedAt { get; set; }
        public AuditReportActivityResponse AuditReportActivity { get; set; }
        public AuditReportUserResponse User { get; set; }
    }
}
