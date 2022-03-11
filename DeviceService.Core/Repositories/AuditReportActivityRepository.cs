using AutoMapper;
using DeviceService.Core.Data.DataContext;
using DeviceService.Core.Dtos.AuditReportActivity;
using DeviceService.Core.Dtos.Global;
using DeviceService.Core.Entities;
using DeviceService.Core.Helpers.Common;
using DeviceService.Core.Interfaces.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeviceService.Core.Repositories
{
    public class AuditReportActivityRepository : IAuditReportActivityRepository
    {
        private readonly DeviceContext _dataContext;
        private readonly IGlobalRepository _globalRepository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditReportActivityRepository(DeviceContext dataContext, IGlobalRepository globalRepository, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _dataContext = dataContext;
            _globalRepository = globalRepository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ReturnResponse> CreateAuditReportActivity(List<AuditReportActivityRequest> auditReportActivities)
        {
            if ((auditReportActivities == null) || (!auditReportActivities.Any()))
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.ObjectNull,
                    StatusMessage = Utils.StatusMessageObjectNull
                };
            }

            var auditReportActivitiesToAdd = _mapper.Map<List<AuditReportActivity>>(auditReportActivities);
            foreach(var t in auditReportActivities)
            {
                var functionality = await _globalRepository.Get<Functionality>(t.FunctionalityId);
                if(functionality == null)
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.NotFound,
                        StatusMessage = Utils.StatusMessageNotFound
                    };
                }
            }

            var creationResult = await _globalRepository.Add(auditReportActivitiesToAdd);
            if (!creationResult)
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
                ObjectValue = auditReportActivitiesToAdd
            };
        }

        public async Task<ReturnResponse> DeleteAuditReportActivity(List<int> auditReportActivitiesIds)
        {
            if((auditReportActivitiesIds == null) || (!auditReportActivitiesIds.Any()))
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.ObjectNull,
                    StatusMessage = Utils.StatusMessageObjectNull
                };
            }

            var auditReportActivitiesToDelete = new List<AuditReportActivity>();
            foreach(var t in auditReportActivitiesIds)
            {
                var auditReportActivity = await _dataContext.AuditReportActivity.FirstOrDefaultAsync(a => a.AuditReportActivityId == t);
                if(auditReportActivity == null)
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.NotFound,
                        StatusMessage = Utils.StatusMessageNotFound
                    };
                }

                //auditReportActivity.DeletedAt = DateTimeOffset.Now;
                auditReportActivitiesToDelete.Add(auditReportActivity);
            }

            var deletionResult = _globalRepository.Delete(auditReportActivitiesToDelete);
            if(!deletionResult)
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
                ObjectValue = auditReportActivitiesToDelete
            };
        }

        public async Task<ReturnResponse> GetAuditReportActivity(int auditReportActivityId)
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
            if(userTypeVal != Utils.UserType_User)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.BadRequest,
                    StatusMessage = Utils.StatusMessageBadRequest
                };
            }

            var auditReportActivity = await _dataContext.AuditReportActivity.Where(a => a.AuditReportActivityId == auditReportActivityId).Include(b => b.Functionality).FirstOrDefaultAsync();
            if(auditReportActivity == null)
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
                ObjectValue = auditReportActivity
            };
        }

        public async Task<ReturnResponse> GetAuditReportActivity()
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
            if (userTypeVal != Utils.UserType_User)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.BadRequest,
                    StatusMessage = Utils.StatusMessageBadRequest
                };
            }

            var auditReportActivities = await _dataContext.AuditReportActivity.Include(a => a.Functionality).ToListAsync();

            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                StatusMessage = Utils.StatusMessageSuccess,
                ObjectValue = auditReportActivities
            };
        }

        public async Task<ReturnResponse> UpdateAuditReportActivity(int auditReportActivityId, AuditReportActivityToUpdate auditReportActivity)
        {
            if(auditReportActivityId != auditReportActivity.AuditReportActivityId)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.BadRequest,
                    StatusMessage = Utils.StatusMessageBadRequest
                };
            }

            if(auditReportActivity == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.ObjectNull,
                    StatusMessage = Utils.StatusMessageObjectNull
                };
            }

            var oldAuditReportActivity = await _dataContext.AuditReportActivity.FirstOrDefaultAsync(a => a.AuditReportActivityId == auditReportActivityId);
            if(oldAuditReportActivity == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotFound,
                    StatusMessage = Utils.StatusMessageNotFound
                };
            }

            var updatedAuditReportActivity = _mapper.Map(auditReportActivity, oldAuditReportActivity);
            var updateResult = _globalRepository.Update(updatedAuditReportActivity);
            if (!updateResult)
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
                ObjectValue = updatedAuditReportActivity
            };
        }
    }
}
