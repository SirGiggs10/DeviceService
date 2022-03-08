using Ayuda_Help_Desk.Dtos.AuditReport;
using Ayuda_Help_Desk.Dtos.General;
using Ayuda_Help_Desk.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ayuda_Help_Desk.Interfaces
{
    public interface IAuditReportRepository
    {
        public Task<ReturnResponse> CreateAuditReport(AuditReportRequest auditReportRequest);
        public Task<ReturnResponse> GetAuditReport(int auditReportId);
        public Task<ReturnResponse> GetAuditReportForUser(int userId, UserParams userParams, AuditReportDateRangeRequest auditReportDateRangeRequest);
        public Task<ReturnResponse> DeleteAuditReport(List<int> auditReportIds);
    }
}
