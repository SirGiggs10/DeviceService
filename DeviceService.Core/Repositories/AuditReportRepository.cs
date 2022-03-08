using AutoMapper;
using Ayuda_Help_Desk.Data;
using Ayuda_Help_Desk.Dtos.AuditReport;
using Ayuda_Help_Desk.Dtos.General;
using Ayuda_Help_Desk.Helpers;
using Ayuda_Help_Desk.Interfaces;
using Ayuda_Help_Desk.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Ayuda_Help_Desk.Repositories
{
    public class AuditReportRepository : IAuditReportRepository
    {
        private readonly IGlobalRepository _globalRepository;
        private readonly DataContext _dataContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;

        public AuditReportRepository(IGlobalRepository globalRepository, DataContext dataContext, IHttpContextAccessor httpContextAccessor, IMapper mapper, UserManager<User> userManager)
        {
            _globalRepository = globalRepository;
            _dataContext = dataContext;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
            _userManager = userManager;
        }

        public async Task<ReturnResponse> CreateAuditReport(AuditReportRequest auditReportRequest)
        {
            if(auditReportRequest == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.ObjectNull,
                    StatusMessage = Utils.StatusMessageObjectNull
                };
            }

            var userIdVal = 0;
            if(auditReportRequest.UserId.HasValue)
            {
                userIdVal = auditReportRequest.UserId.Value;
            }
            else
            {
                var userType = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == Utils.ClaimType_UserType);
                var userId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name);
                if ((userType == null) || (userId == null))
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.NotFound,
                        StatusMessage = Utils.StatusMessageNotFound
                    };
                }

                var userTypeVal = Convert.ToInt32(userType.Value);
                userIdVal = Convert.ToInt32(userId.Value);

                if ((userTypeVal != Utils.Staff) && (userTypeVal != Utils.Customer))
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.BadRequest,
                        StatusMessage = Utils.StatusMessageBadRequest
                    };
                }
            }           

            var functionality = await _dataContext.Functionality.Where(a => a.FunctionalityName == auditReportRequest.AuditReportActivityFunctionalityName).Include(b => b.AuditReportActivity).FirstOrDefaultAsync();
            if(functionality == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotFound,
                    StatusMessage = Utils.StatusMessageNotFound
                };
            }

            if(functionality.AuditReportActivity == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotFound,
                    StatusMessage = Utils.StatusMessageNotFound
                };
            }

            var auditReport = new AuditReport();
            auditReport.UserId = userIdVal;
            auditReport.AuditReportActivityId = functionality.AuditReportActivity.AuditReportActivityId;
            auditReport.AuditReportActivityResourceId = JsonConvert.SerializeObject(auditReportRequest.AuditReportActivityResourceId, new JsonSerializerSettings()
            {
                //ReferenceLoopHandling = ReferenceLoopHandling.Error,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                Formatting = Formatting.None
            });

            var creationResult =_globalRepository.Add(auditReport);
            if(!creationResult)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotSucceeded,
                    StatusMessage = Utils.StatusMessageNotSucceeded
                };
            }

            var saveResult = await _globalRepository.SaveAll();
            if(!saveResult.HasValue)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.SaveNoRowAffected,
                    StatusMessage = Utils.StatusMessageSaveNoRowAffected
                };
            }

            if(!saveResult.Value)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.SaveError,
                    StatusMessage = Utils.StatusMessageSaveError
                };
            }

            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                StatusMessage = Utils.StatusMessageSuccess,
                ObjectValue = auditReport
            };
        }

        public async Task<ReturnResponse> DeleteAuditReport(List<int> auditReportIds)
        {
            if((auditReportIds == null) || (!auditReportIds.Any()))
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.ObjectNull,
                    StatusMessage = Utils.StatusMessageObjectNull
                };
            }

            var auditReportsToDelete = new List<AuditReport>();
            foreach(var t in auditReportIds)
            {
                var auditReport = await _dataContext.AuditReport.Where(a => a.AuditReportId == t).FirstOrDefaultAsync();
                if(auditReport == null)
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.NotFound,
                        StatusMessage = Utils.StatusMessageNotFound
                    };
                }

                auditReport.DeletedAt = DateTimeOffset.Now;
                auditReportsToDelete.Add(auditReport);
            }

            var deletionResult = _globalRepository.Update(auditReportsToDelete);
            if(!deletionResult)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotSucceeded,
                    StatusMessage = Utils.StatusMessageNotSucceeded
                };
            }

            var saveResult = await _globalRepository.SaveAll();
            if (!saveResult.HasValue)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.SaveNoRowAffected,
                    StatusMessage = Utils.StatusMessageSaveNoRowAffected
                };
            }

            if (!saveResult.Value)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.SaveError,
                    StatusMessage = Utils.StatusMessageSaveError
                };
            }

            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                StatusMessage = Utils.StatusMessageSuccess,
                ObjectValue = auditReportsToDelete
            };
        }

        public async Task<ReturnResponse> GetAuditReport(int auditReportId)
        {
            var auditReport = await _dataContext.AuditReport.Where(a => a.AuditReportId == auditReportId).Include(b => b.AuditReportActivity).Include(c => c.User).FirstOrDefaultAsync();
            if(auditReport == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotFound,
                    StatusMessage = Utils.StatusMessageNotFound
                };
            }

            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                StatusMessage = Utils.StatusMessageSuccess,
                ObjectValue = auditReport
            };
        }

        public async Task<ReturnResponse> GetAuditReportForUser(int userId, UserParams userParams, AuditReportDateRangeRequest auditReportDateRangeRequest)
        {
            var userType = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == Utils.ClaimType_UserType);
            if ((userType == null))
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotFound,
                    StatusMessage = Utils.StatusMessageNotFound
                };
            }

            var userTypeVal = Convert.ToInt32(userType.Value);
            if(userTypeVal != Utils.Staff)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.BadRequest,
                    StatusMessage = Utils.StatusMessageBadRequest
                };
            }

            var userDetails = await _userManager.Users.FirstOrDefaultAsync(a => a.Id == userId);
            if(userDetails == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotFound,
                    StatusMessage = Utils.StatusMessageNotFound
                };
            }

            if(userDetails.UserType == Utils.Staff)
            {
                var userDetailsProfile = await _dataContext.Staff.Where(a => a.StaffId == userDetails.UserTypeId).FirstOrDefaultAsync();
            }
            else if(userDetails.UserType == Utils.Customer)
            {
                var userDetailsProfile = await _dataContext.Customer.Where(a => a.CustomerId == userDetails.UserTypeId).FirstOrDefaultAsync();
            }
            else
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.BadRequest,
                    StatusMessage = Utils.StatusMessageBadRequest
                };
            }

            PagedList<AuditReport> pagedListOfAuditReports = null;
            if (auditReportDateRangeRequest != null)
            {
                if((auditReportDateRangeRequest.StartDateTime == null) || (auditReportDateRangeRequest.EndDateTime == null))
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.ObjectNull,
                        StatusMessage = Utils.StatusMessageObjectNull
                    };
                }

                var auditReportsForUserByDate = _dataContext.AuditReport.Where(a => (a.UserId == userId) && ((a.CreatedAt.CompareTo(auditReportDateRangeRequest.StartDateTime) >= 0) && (a.CreatedAt.CompareTo(auditReportDateRangeRequest.EndDateTime) <= 0))).Include(b => b.AuditReportActivity).OrderByDescending(c => c.CreatedAt);
                pagedListOfAuditReports = await PagedList<AuditReport>.CreateAsync(auditReportsForUserByDate, userParams.PageNumber, userParams.PageSize);
            }
            else
            {
                var auditReportsForUser = _dataContext.AuditReport.Where(a => a.UserId == userId).Include(b => b.AuditReportActivity).OrderByDescending(c => c.CreatedAt);
                pagedListOfAuditReports = await PagedList<AuditReport>.CreateAsync(auditReportsForUser, userParams.PageNumber, userParams.PageSize);
            }

            var listOfAuditReportsToReturn = pagedListOfAuditReports.ToList();
            _httpContextAccessor.HttpContext.Response.AddPagination(pagedListOfAuditReports.CurrentPage, pagedListOfAuditReports.PageSize, pagedListOfAuditReports.TotalCount, pagedListOfAuditReports.TotalPages);

            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                StatusMessage = Utils.StatusMessageSuccess,
                ObjectValue = listOfAuditReportsToReturn
            };
        }
    }
}
