using Ayuda_Help_Desk.Dtos.AuditReportActivity;
using Ayuda_Help_Desk.Dtos.General;
using Ayuda_Help_Desk.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ayuda_Help_Desk.Interfaces
{
    public interface IAuditReportActivityRepository
    {
        public Task<ReturnResponse> CreateAuditReportActivity(List<AuditReportActivityRequest> auditReportActivities);
        public Task<ReturnResponse> GetAuditReportActivity(int auditReportActivityId);
        public Task<ReturnResponse> GetAuditReportActivity();
        public Task<ReturnResponse> UpdateAuditReportActivity(int auditReportActivityId, AuditReportActivityToUpdate auditReportActivity);
        public Task<ReturnResponse> DeleteAuditReportActivity(List<int> auditReportActivitiesIds);
    }
}
