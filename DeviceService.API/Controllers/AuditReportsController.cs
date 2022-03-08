using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ayuda_Help_Desk.Data;
using Ayuda_Help_Desk.Models;
using Ayuda_Help_Desk.Helpers;
using Ayuda_Help_Desk.Interfaces;
using AutoMapper;
using Ayuda_Help_Desk.Dtos.AuditReport;
using Microsoft.AspNetCore.Authorization;
using Ayuda_Help_Desk.Dtos.UserManagement;
using Ayuda_Help_Desk.Dtos.General;

namespace Ayuda_Help_Desk.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class AuditReportsController : ControllerBase
    {
        private readonly DataContext _dataContext;
        private readonly IAuditReportRepository _auditReportRepository;
        private readonly IMapper _mapper;

        public AuditReportsController(DataContext dataContext, IAuditReportRepository auditReportRepository, IMapper mapper)
        {
            _dataContext = dataContext;
            _auditReportRepository = auditReportRepository;
            _mapper = mapper;
        }

        // GET: api/AuditReports/User/3
        [RequiredFunctionalityName("GetAuditReportsForUser")]
        [HttpGet("User/{userId}")]
        public async Task<ActionResult<IEnumerable<AuditReport>>> GetAuditReportsForUser([FromRoute] int userId, [FromQuery] UserParams userParams)
        {
            var result = await _auditReportRepository.GetAuditReportForUser(userId, userParams, null);
            if (result.StatusCode == Utils.Success)
            {
                result.ObjectValue = _mapper.Map<List<AuditReportResponse>>((List<AuditReport>)result.ObjectValue);
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "GetAuditReportsForUser",
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

        // POST: api/AuditReports/User/3/Date
        [RequiredFunctionalityName("GetAuditReportsForUserByDateRange")]
        [HttpPost("User/{userId}/Date")]
        public async Task<ActionResult<IEnumerable<AuditReport>>> GetAuditReportsForUserByDateRange([FromRoute] int userId, [FromQuery] UserParams userParams, [FromBody] AuditReportDateRangeRequest auditReportDateRangeRequest)
        {
            var result = await _auditReportRepository.GetAuditReportForUser(userId, userParams, auditReportDateRangeRequest);
            if (result.StatusCode == Utils.Success)
            {
                result.ObjectValue = _mapper.Map<List<AuditReportResponse>>((List<AuditReport>)result.ObjectValue);
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "GetAuditReportsForUserByDateRange",
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

        // GET: api/AuditReports/5
        [RequiredFunctionalityName("GetAuditReport")]
        [HttpGet("{id}")]
        public async Task<ActionResult<AuditReport>> GetAuditReport(int id)
        {
            var result = await _auditReportRepository.GetAuditReport(id);
            if (result.StatusCode == Utils.Success)
            {
                result.ObjectValue = _mapper.Map<AuditReportResponse>((AuditReport)result.ObjectValue);
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "GetAuditReport",
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

        // PUT: api/AuditReports/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [RequiredFunctionalityName("PutAuditReport")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAuditReport(int id, AuditReport auditReport)
        {
            throw new NotImplementedException();
        }

        // POST: api/AuditReports
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [RequiredFunctionalityName("PostAuditReport")]
        [HttpPost]
        public async Task<ActionResult<AuditReport>> PostAuditReport(AuditReport auditReport)
        {
            throw new NotImplementedException();
        }

        // DELETE: api/AuditReports/Delete
        [RequiredFunctionalityName("DeleteAuditReport")]
        [HttpPost("Delete")]
        public async Task<ActionResult<AuditReport>> DeleteAuditReport(List<int> auditReportIds)
        {
            var dbTransaction = await _dataContext.Database.BeginTransactionAsync();

            var result = await _auditReportRepository.DeleteAuditReport(auditReportIds);
            if(result.StatusCode == Utils.Success)
            {
                var auditReports = _mapper.Map<List<AuditReportResponse>>((List<AuditReport>)result.ObjectValue);
                result.ObjectValue = auditReports;
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "DeleteAuditReport",
                    AuditReportActivityResourceId = auditReports.Select(a => a.AuditReportId).ToList()
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
