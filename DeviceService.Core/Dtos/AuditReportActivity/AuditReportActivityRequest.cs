using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeviceService.Core.Dtos.AuditReportActivity
{
    public class AuditReportActivityRequest
    {
        //public string AuditReportActivityName { get; set; }
        public int FunctionalityId { get; set; }
        public string AuditReportActivityDescription { get; set; }
        public string AuditReportActivityViewUrl { get; set; }
        public string FrontendRoute { get; set; }
    }
}
