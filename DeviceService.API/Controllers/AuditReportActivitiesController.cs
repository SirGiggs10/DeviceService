using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using DeviceService.Core.Helpers.Common;

namespace DeviceService.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class AuditReportActivitiesController : ControllerBase
    {
        private readonly DataContext _dataContext;
        private readonly IAuditReportActivityRepository _auditReportActivityRepository;
        private readonly IMapper _mapper;
        private readonly IAuditReportRepository _auditReportRepository;

        public AuditReportActivitiesController(DataContext dataContext, IAuditReportActivityRepository auditReportActivityRepository, IMapper mapper, IAuditReportRepository auditReportRepository)
        {
            _dataContext = dataContext;
            _auditReportActivityRepository = auditReportActivityRepository;
            _mapper = mapper;
            _auditReportRepository = auditReportRepository;
        }

        // GET: api/AuditReportActivities
        [RequiredFunctionalityName("GetAuditReportActivities")]
        [HttpGet]
        public async Task<ActionResult<ControllerReturnResponse>> GetAuditReportActivity()
        {
            var result = await _auditReportActivityRepository.GetAuditReportActivity();

            if (result.StatusCode == Utils.Success)
            {
                result.ObjectValue = _mapper.Map<List<AuditReportActivityResponse>>((List<AuditReportActivity>)result.ObjectValue);
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "GetAuditReportActivities",
                    AuditReportActivityResourceId = new List<int>() { }
                });

                if (auditResult.StatusCode != Utils.Success)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, new ReturnResponse()
                    {
                        StatusCode = Utils.AuditReportError,
                        StatusMessage = Utils.StatusMessageAuditReportError
                    });
                }

                return StatusCode(StatusCodes.Status200OK, result);
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, result);
            }
        }

        // GET: api/AuditReportActivities/5
        [RequiredFunctionalityName("GetAuditReportActivity")]
        [HttpGet("{id}")]
        public async Task<ActionResult<ReturnResponse>> GetAuditReportActivity(int id)
        {
            var result = await _auditReportActivityRepository.GetAuditReportActivity(id);

            if (result.StatusCode == Utils.Success)
            {
                var auditReportActivity = _mapper.Map<AuditReportActivityResponse>((AuditReportActivity)result.ObjectValue);
                result.ObjectValue = auditReportActivity;
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "GetAuditReportActivity",
                    AuditReportActivityResourceId = new List<int>() { auditReportActivity.AuditReportActivityId }
                });

                if (auditResult.StatusCode != Utils.Success)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, new ReturnResponse()
                    {
                        StatusCode = Utils.AuditReportError,
                        StatusMessage = Utils.StatusMessageAuditReportError
                    });
                }

                return StatusCode(StatusCodes.Status200OK, result);
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, result);
            }
        }

        // PUT: api/AuditReportActivities/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [RequiredFunctionalityName("PutAuditReportActivity")]
        [HttpPut("{auditReportActivityId}")]
        public async Task<ActionResult<ReturnResponse>> PutAuditReportActivity(int auditReportActivityId, [FromBody] AuditReportActivityToUpdate auditReportActivityToUpdate)
        {
            var dbTransaction = await _dataContext.Database.BeginTransactionAsync();
            var result = await _auditReportActivityRepository.UpdateAuditReportActivity(auditReportActivityId, auditReportActivityToUpdate);

            if (result.StatusCode == Utils.Success)
            {
                var auditReportActivity = _mapper.Map<AuditReportActivityResponse>((AuditReportActivity)result.ObjectValue);
                result.ObjectValue = auditReportActivity;
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "PutAuditReportActivity",
                    AuditReportActivityResourceId = new List<int>() { auditReportActivity.AuditReportActivityId }
                });

                if (auditResult.StatusCode != Utils.Success)
                {
                    await dbTransaction.RollbackAsync();

                    return StatusCode(StatusCodes.Status400BadRequest, new ReturnResponse()
                    {
                        StatusCode = Utils.AuditReportError,
                        StatusMessage = Utils.StatusMessageAuditReportError
                    });
                }

                await dbTransaction.CommitAsync();

                return StatusCode(StatusCodes.Status200OK, result);
            }
            else
            {
                await dbTransaction.RollbackAsync();

                return StatusCode(StatusCodes.Status400BadRequest, result);
            }
        }

        // POST: api/AuditReportActivities
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [RequiredFunctionalityName("PostAuditReportActivity")]
        [HttpPost]
        public async Task<ActionResult<ReturnResponse>> PostAuditReportActivity([FromBody] List<AuditReportActivityRequest> auditReportActivityRequests)
        {
            var dbTransaction = await _dataContext.Database.BeginTransactionAsync();
            var result = await _auditReportActivityRepository.CreateAuditReportActivity(auditReportActivityRequests);

            if(result.StatusCode == Utils.Success)
            {
                var auditReportActivities = _mapper.Map<List<AuditReportActivityResponse>>((List<AuditReportActivity>)result.ObjectValue);
                result.ObjectValue = auditReportActivities;
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "PostAuditReportActivity",
                    AuditReportActivityResourceId = auditReportActivities.Select(a => a.AuditReportActivityId).ToList()
                });

                if (auditResult.StatusCode != Utils.Success)
                {
                    await dbTransaction.RollbackAsync();

                    return StatusCode(StatusCodes.Status400BadRequest, new ReturnResponse()
                    {
                        StatusCode = Utils.AuditReportError,
                        StatusMessage = Utils.StatusMessageAuditReportError
                    });
                }
                
                await dbTransaction.CommitAsync();

                return StatusCode(StatusCodes.Status200OK, result);
            }
            else
            {
                await dbTransaction.RollbackAsync();

                return StatusCode(StatusCodes.Status400BadRequest, result);
            }
        }

        // DELETE: api/AuditReportActivities/Delete
        [RequiredFunctionalityName("DeleteAuditReportActivity")]
        [HttpPost("Delete")]
        public async Task<ActionResult<ReturnResponse>> DeleteAuditReportActivity([FromBody] List<int> auditReportActivitiesIds)
        {
            var dbTransaction = await _dataContext.Database.BeginTransactionAsync();
            var result = await _auditReportActivityRepository.DeleteAuditReportActivity(auditReportActivitiesIds);

            if (result.StatusCode == Utils.Success)
            {
                var auditReportActivities = _mapper.Map<List<AuditReportActivityResponse>>((List<AuditReportActivity>)result.ObjectValue);
                result.ObjectValue = auditReportActivities;
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "DeleteAuditReportActivity",
                    AuditReportActivityResourceId = auditReportActivities.Select(a => a.AuditReportActivityId).ToList()
                });

                if (auditResult.StatusCode != Utils.Success)
                {
                    await dbTransaction.RollbackAsync();

                    return StatusCode(StatusCodes.Status400BadRequest, new ReturnResponse()
                    {
                        StatusCode = Utils.AuditReportError,
                        StatusMessage = Utils.StatusMessageAuditReportError
                    });
                }

                await dbTransaction.CommitAsync();

                return StatusCode(StatusCodes.Status200OK, result);
            }
            else
            {
                await dbTransaction.RollbackAsync();

                return StatusCode(StatusCodes.Status400BadRequest, result);
            }
        }
    }
}
