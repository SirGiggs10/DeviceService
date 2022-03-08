using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeviceService.Dtos.AuditReportActivity
{
    public class AuditReportActivityToUpdate
    {
        public int AuditReportActivityId { get; set; }
        public int FunctionalityId { get; set; }
        public string AuditReportActivityDescription { get; set; }
        public string AuditReportActivityViewUrl { get; set; }
        public string FrontendRoute { get; set; }
    }
}
