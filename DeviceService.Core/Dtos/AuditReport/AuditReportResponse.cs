using DeviceService.Core.Dtos.AuditReportActivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeviceService.Core.Dtos.AuditReport
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
