using DeviceService.Core.Dtos.AuditReport;
using DeviceService.Core.Dtos.Global;
using DeviceService.Core.Helpers.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeviceService.Core.Interfaces.Repositories
{
    public interface IAuditReportRepository
    {
        public Task<ReturnResponse> CreateAuditReport(AuditReportRequest auditReportRequest);
        public Task<ReturnResponse> GetAuditReport(int auditReportId);
        public Task<ReturnResponse> GetAuditReportForUser(int userId, UserParams userParams, AuditReportDateRangeRequest auditReportDateRangeRequest);
        public Task<ReturnResponse> DeleteAuditReport(List<int> auditReportIds);
    }
}
