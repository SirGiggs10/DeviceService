using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DeviceService.Core.Data.DataContext;
using DeviceService.Core.Helpers.Common;
using DeviceService.Core.Entities;
using DeviceService.Core.Interfaces.Repositories;

namespace DeviceService.Core.Helpers.RoleBasedAccess
{
    public class FunctionalityNameHandler : AuthorizationHandler<FunctionalityNameRequirement>
    {
        private readonly DeviceContext _dataContext;
        private readonly IAuditReportRepository _auditReportRepository;

        public FunctionalityNameHandler(DeviceContext dataContext, IAuditReportRepository auditReportRepository)
        {
            _dataContext = dataContext;
            _auditReportRepository = auditReportRepository;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, FunctionalityNameRequirement requirement)
        {
            if (!context.User.HasClaim(c => c.Type == ClaimTypes.Role))
            {
                return Task.FromResult(0);
            }

            var userType = context.User.Claims.FirstOrDefault(a => a.Type == Utils.ClaimType_UserType);
            var userId = context.User.Claims.FirstOrDefault(a => a.Type == ClaimTypes.Name);
            if (userType == null || userId == null)
            {
                return Task.FromResult(0);
            }

            var userTypeVal = Convert.ToInt32(userType.Value);
            var userIdVal = Convert.ToInt32(userId.Value);

            // check if any of the user's roles has the the functionality name mapped to it.
            var funcName = requirement.FuncName;
            var roles = context.User.FindAll(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
            var funcRole = _dataContext.FunctionalityRole.Where(c => c.FunctionalityName == funcName).Select(r => r.RoleName).ToList();

            foreach (var role in roles)
            {
                if (funcRole.Contains(role))
                {
                    context.Succeed(requirement);
                    return Task.FromResult(0);
                }
            }

            //AUDIT THIS ACTIVITY FOR THE USER
            var functionality = _dataContext.Functionality.Where(a => a.FunctionalityName == "FailedRouteAuthorization").Include(b => b.AuditReportActivity).FirstOrDefault();
            if(functionality == null)
            {
                return Task.FromResult(0);
            }

            if(functionality.AuditReportActivity == null)
            {
                return Task.FromResult(0);
            }

            var auditReport = _dataContext.AuditReport.Add(new AuditReport()
            {
                AuditReportActivityId = functionality.AuditReportActivity.AuditReportActivityId,
                AuditReportActivityResourceId = JsonConvert.SerializeObject(new List<int>() { }),
                UserId = userIdVal
            });

            var saveResult = _dataContext.SaveChanges();
            if(saveResult <= 0)
            {
                return Task.FromResult(0);
            }

            return Task.FromResult(0);

            //if(funcRole != null)
            //{
            //    var intersect = funcRole.Intersect(roles);
            //    if(intersect.Count() != 0)
            //    {
            //    }
            //}
        }
    }
}
