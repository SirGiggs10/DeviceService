using Ayuda_Help_Desk.Dtos.RoleFunctionality;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeviceService.Dtos.AuditReportActivity
{
    public class AuditReportActivityResponse
    {
        public int AuditReportActivityId { get; set; }
        //public string AuditReportActivityName { get; set; }
        public int FunctionalityId { get; set; }
        public string AuditReportActivityDescription { get; set; }
        public string AuditReportActivityViewUrl { get; set; }
        public string FrontendRoute { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public FunctionalityResponse Functionality { get; set; }
    }
}
