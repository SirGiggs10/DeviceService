using DeviceService.Core.Dtos.AuditReportActivity;
using DeviceService.Core.Dtos.Global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeviceService.Core.Interfaces.Repositories
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
