using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeviceService.Dtos.AuditReport
{
    public class AuditReportRequest
    {
        public string AuditReportActivityFunctionalityName { get; set; }
        public List<int> AuditReportActivityResourceId { get; set; }
        public int? UserId { get; set; }
    }
}
