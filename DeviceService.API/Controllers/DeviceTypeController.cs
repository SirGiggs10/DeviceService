using AutoMapper;
using DeviceService.Core.Data.DataContext;
using DeviceService.Core.Dtos.AuditReport;
using DeviceService.Core.Dtos.DeviceType;
using DeviceService.Core.Dtos.Global;
using DeviceService.Core.Helpers.Common;
using DeviceService.Core.Helpers.Logging.Logger;
using DeviceService.Core.Helpers.Pagination;
using DeviceService.Core.Helpers.RoleBasedAccess;
using DeviceService.Core.Interfaces.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DeviceService.API.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class DeviceTypeController : ControllerBase
    {
        private readonly DeviceContext _deviceContext;
        private readonly IDeviceTypeRepository _deviceTypeRepository;
        private readonly IMapper _mapper;
        private readonly IAuditReportRepository _auditReportRepository;

        public DeviceTypeController(DeviceContext deviceContext, IDeviceTypeRepository deviceTypeRepository, IMapper mapper, IAuditReportRepository auditReportRepository)
        {
            _deviceContext = deviceContext;
            _deviceTypeRepository = deviceTypeRepository;
            _mapper = mapper;
            _auditReportRepository = auditReportRepository;
        }

        // GET api/DeviceType/1
        /// <summary>
        /// GET SINGLE DEVICE TYPE BY ID
        /// </summary>
        /// <param name="deviceTypeId">Device Type Id</param>
        /// <returns>Device Type Details</returns>
        /// <response code="200">Returns Device Type Details</response>
        /// <response code="400">If Request is Bad</response>
        /// <response code="404">If an Object is not Found</response>
        /// <response code="500">If Unknown Error Occurs</response>
        [ProducesResponseType(typeof(ControllerReturnResponse<DeviceTypeResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status500InternalServerError)]
        [Consumes("application/json")]
        [RequiredFunctionalityName("GetDeviceType")]
        [HttpGet("{deviceTypeId}")]
        public async Task<ActionResult<ControllerReturnResponse<DeviceTypeResponse>>> GetDeviceType([FromRoute][Required] int deviceTypeId)
        {
            var result = await _deviceTypeRepository.GetDeviceType(deviceTypeId);

            //LOG THE ACTIONS
            LogWriter.WriteLog(result.Logs);

            if (result.StatusCode == Utils.Success)
            {
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "GetDeviceType",
                    AuditReportActivityResourceId = new List<int>() { result.ObjectValue.DeviceTypeId }
                });

                if (auditResult.StatusCode != Utils.Success)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, new ControllerReturnResponse<DeviceTypeResponse>()
                    {
                        ResponseCode = HttpStatusCode.BadRequest,
                        ResponseDescription = Utils.StatusMessageAuditReportError,
                        StatusCode = Utils.AuditReportError,
                        StatusMessage = Utils.StatusMessageAuditReportError
                    });
                }

                return StatusCode(StatusCodes.Status200OK, new ControllerReturnResponse<DeviceTypeResponse>()
                {
                    ResponseCode = HttpStatusCode.OK,
                    ResponseDescription = result.StatusMessage,
                    StatusCode = result.StatusCode,
                    StatusMessage = result.StatusMessage,
                    ObjectValue = result.ObjectValue
                });
            }
            else if (result.StatusCode == Utils.NotFound)
            {
                return StatusCode(StatusCodes.Status404NotFound, new ControllerReturnResponse<DeviceTypeResponse>()
                {
                    ResponseCode = HttpStatusCode.NotFound,
                    ResponseDescription = result.StatusMessage,
                    StatusCode = result.StatusCode,
                    StatusMessage = result.StatusMessage
                });
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ControllerReturnResponse<DeviceTypeResponse>()
                {
                    ResponseCode = HttpStatusCode.BadRequest,
                    ResponseDescription = result.StatusMessage,
                    StatusCode = result.StatusCode,
                    StatusMessage = result.StatusMessage
                });
            }
        }

        // GET api/DeviceType?PageNumber=1&PageSize=4
        /// <summary>
        /// GET ALL DEVICE TYPES
        /// </summary>
        /// <param name="deviceTypeId">Device Type Id</param>
        /// <returns>Device Types Details</returns>
        /// <response code="200">Returns Device Types Details</response>
        /// <response code="400">If Request is Bad</response>
        /// <response code="404">If an Object is not Found</response>
        /// <response code="500">If Unknown Error Occurs</response>
        [ProducesResponseType(typeof(ControllerReturnResponse<List<DeviceTypeResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status500InternalServerError)]
        [Consumes("application/json")]
        [RequiredFunctionalityName("GetDeviceTypes")]
        [HttpGet]
        public async Task<ActionResult<ControllerReturnResponse<List<DeviceTypeResponse>>>> GetDeviceTypes([FromQuery] UserParams userParams)
        {
            var result = await _deviceTypeRepository.GetDeviceTypes(userParams);

            //LOG THE ACTIONS
            LogWriter.WriteLog(result.Logs);

            if (result.StatusCode == Utils.Success)
            {
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "GetDeviceTypes",
                    AuditReportActivityResourceId = new List<int>() { }
                });

                if (auditResult.StatusCode != Utils.Success)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, new ControllerReturnResponse<List<DeviceTypeResponse>>()
                    {
                        ResponseCode = HttpStatusCode.BadRequest,
                        ResponseDescription = Utils.StatusMessageAuditReportError,
                        StatusCode = Utils.AuditReportError,
                        StatusMessage = Utils.StatusMessageAuditReportError
                    });
                }

                return StatusCode(StatusCodes.Status200OK, new ControllerReturnResponse<List<DeviceTypeResponse>>()
                {
                    ResponseCode = HttpStatusCode.OK,
                    ResponseDescription = result.StatusMessage,
                    StatusCode = result.StatusCode,
                    StatusMessage = result.StatusMessage,
                    ObjectValue = result.ObjectValue
                });
            }
            else if (result.StatusCode == Utils.NotFound)
            {
                return StatusCode(StatusCodes.Status404NotFound, new ControllerReturnResponse<List<DeviceTypeResponse>>()
                {
                    ResponseCode = HttpStatusCode.NotFound,
                    ResponseDescription = result.StatusMessage,
                    StatusCode = result.StatusCode,
                    StatusMessage = result.StatusMessage
                });
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ControllerReturnResponse<List<DeviceTypeResponse>>()
                {
                    ResponseCode = HttpStatusCode.BadRequest,
                    ResponseDescription = result.StatusMessage,
                    StatusCode = result.StatusCode,
                    StatusMessage = result.StatusMessage
                });
            }
        }

        // PUT api/DeviceType/1
        /// <summary>
        /// UPDATE DEVICE TYPE
        /// </summary>
        /// <param name="deviceTypeId">Device Type Id</param>
        /// <param name="deviceTypeToUpdate">Device Type Request Body</param>
        /// <returns>Device Type Details</returns>
        /// <response code="200">Returns Device Type Details</response>
        /// <response code="400">If Request is Bad</response>
        /// <response code="404">If an Object is not Found</response>
        /// <response code="500">If Unknown Error Occurs</response>
        [ProducesResponseType(typeof(ControllerReturnResponse<DeviceTypeResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status500InternalServerError)]
        [Consumes("application/json")]
        [RequiredFunctionalityName("PutDeviceType")]
        [HttpPut("{deviceTypeId}")]
        public async Task<ActionResult<ControllerReturnResponse<DeviceTypeResponse>>> PutDeviceType([FromRoute][Required] int deviceTypeId, [FromBody] DeviceTypeToUpdate deviceTypeToUpdate)
        {
            var dbTransaction = await _deviceContext.Database.BeginTransactionAsync();

            var result = await _deviceTypeRepository.UpdateDeviceType(deviceTypeId, deviceTypeToUpdate);

            //LOG THE ACTIONS
            LogWriter.WriteLog(result.Logs);

            if (result.StatusCode == Utils.Success)
            {
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "PutDeviceType",
                    AuditReportActivityResourceId = new List<int>() { result.ObjectValue.DeviceTypeId }
                });

                if (auditResult.StatusCode != Utils.Success)
                {
                    await dbTransaction.RollbackAsync();

                    return StatusCode(StatusCodes.Status400BadRequest, new ControllerReturnResponse<DeviceTypeResponse>()
                    {
                        ResponseCode = HttpStatusCode.BadRequest,
                        ResponseDescription = Utils.StatusMessageAuditReportError,
                        StatusCode = Utils.AuditReportError,
                        StatusMessage = Utils.StatusMessageAuditReportError
                    });
                }

                await dbTransaction.CommitAsync();

                return StatusCode(StatusCodes.Status200OK, new ControllerReturnResponse<DeviceTypeResponse>()
                {
                    ResponseCode = HttpStatusCode.OK,
                    ResponseDescription = result.StatusMessage,
                    StatusCode = result.StatusCode,
                    StatusMessage = result.StatusMessage,
                    ObjectValue = result.ObjectValue
                });
            }
            else if (result.StatusCode == Utils.NotFound)
            {
                await dbTransaction.RollbackAsync();

                return StatusCode(StatusCodes.Status404NotFound, new ControllerReturnResponse<DeviceTypeResponse>()
                {
                    ResponseCode = HttpStatusCode.NotFound,
                    ResponseDescription = result.StatusMessage,
                    StatusCode = result.StatusCode,
                    StatusMessage = result.StatusMessage
                });
            }
            else
            {
                await dbTransaction.RollbackAsync();

                return StatusCode(StatusCodes.Status400BadRequest, new ControllerReturnResponse<DeviceTypeResponse>()
                {
                    ResponseCode = HttpStatusCode.BadRequest,
                    ResponseDescription = result.StatusMessage,
                    StatusCode = result.StatusCode,
                    StatusMessage = result.StatusMessage
                });
            }
        }

        // POST api/DeviceType
        /// <summary>
        /// CREATE DEVICE TYPE
        /// </summary>
        /// <param name="deviceTypeRequest">Device Type Request Body</param>
        /// <returns>Device Type Details</returns>
        /// <response code="200">Returns Device Type Details</response>
        /// <response code="400">If Request is Bad</response>
        /// <response code="404">If an Object is not Found</response>
        /// <response code="500">If Unknown Error Occurs</response>
        [ProducesResponseType(typeof(ControllerReturnResponse<DeviceTypeResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status500InternalServerError)]
        [Consumes("application/json")]
        [RequiredFunctionalityName("PostDeviceType")]
        [HttpPost]
        public async Task<ActionResult<ControllerReturnResponse<DeviceTypeResponse>>> PostDeviceType([FromBody] DeviceTypeRequest deviceTypeRequest)
        {
            var dbTransaction = await _deviceContext.Database.BeginTransactionAsync();

            var result = await _deviceTypeRepository.CreateDeviceType(deviceTypeRequest);

            //LOG THE ACTIONS
            LogWriter.WriteLog(result.Logs);

            if (result.StatusCode == Utils.Success)
            {
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "PostDeviceType",
                    AuditReportActivityResourceId = new List<int>() { result.ObjectValue.DeviceTypeId }
                });

                if (auditResult.StatusCode != Utils.Success)
                {
                    await dbTransaction.RollbackAsync();

                    return StatusCode(StatusCodes.Status400BadRequest, new ControllerReturnResponse<DeviceTypeResponse>()
                    {
                        ResponseCode = HttpStatusCode.BadRequest,
                        ResponseDescription = Utils.StatusMessageAuditReportError,
                        StatusCode = Utils.AuditReportError,
                        StatusMessage = Utils.StatusMessageAuditReportError
                    });
                }

                await dbTransaction.CommitAsync();

                return StatusCode(StatusCodes.Status200OK, new ControllerReturnResponse<DeviceTypeResponse>()
                {
                    ResponseCode = HttpStatusCode.OK,
                    ResponseDescription = result.StatusMessage,
                    StatusCode = result.StatusCode,
                    StatusMessage = result.StatusMessage,
                    ObjectValue = result.ObjectValue
                });
            }
            else if (result.StatusCode == Utils.NotFound)
            {
                await dbTransaction.RollbackAsync();

                return StatusCode(StatusCodes.Status404NotFound, new ControllerReturnResponse<DeviceTypeResponse>()
                {
                    ResponseCode = HttpStatusCode.NotFound,
                    ResponseDescription = result.StatusMessage,
                    StatusCode = result.StatusCode,
                    StatusMessage = result.StatusMessage
                });
            }
            else
            {
                await dbTransaction.RollbackAsync();

                return StatusCode(StatusCodes.Status400BadRequest, new ControllerReturnResponse<DeviceTypeResponse>()
                {
                    ResponseCode = HttpStatusCode.BadRequest,
                    ResponseDescription = result.StatusMessage,
                    StatusCode = result.StatusCode,
                    StatusMessage = result.StatusMessage
                });
            }
        }

        // POST api/DeviceType/Delete
        /// <summary>
        /// DELETE DEVICE TYPES
        /// </summary>
        /// <param name="deviceTypeIds">Device Types Ids to Delete</param>
        /// <returns>Device Types Details</returns>
        /// <response code="200">Returns Device Types Details</response>
        /// <response code="400">If Request is Bad</response>
        /// <response code="404">If an Object is not Found</response>
        /// <response code="500">If Unknown Error Occurs</response>
        [ProducesResponseType(typeof(ControllerReturnResponse<List<DeviceTypeResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status500InternalServerError)]
        [Consumes("application/json")]
        [RequiredFunctionalityName("PostDeleteDeviceType")]
        [HttpPost("Delete")]
        public async Task<ActionResult<ControllerReturnResponse<List<DeviceTypeResponse>>>> PostDeleteDeviceType([FromBody] List<int> deviceTypeIds)
        {
            var dbTransaction = await _deviceContext.Database.BeginTransactionAsync();

            var result = await _deviceTypeRepository.DeleteDeviceType(deviceTypeIds);

            //LOG THE ACTIONS
            LogWriter.WriteLog(result.Logs);

            if (result.StatusCode == Utils.Success)
            {
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "PostDeleteDeviceType",
                    AuditReportActivityResourceId = deviceTypeIds
                });

                if (auditResult.StatusCode != Utils.Success)
                {
                    await dbTransaction.RollbackAsync();

                    return StatusCode(StatusCodes.Status400BadRequest, new ControllerReturnResponse<List<DeviceTypeResponse>>()
                    {
                        ResponseCode = HttpStatusCode.BadRequest,
                        ResponseDescription = Utils.StatusMessageAuditReportError,
                        StatusCode = Utils.AuditReportError,
                        StatusMessage = Utils.StatusMessageAuditReportError
                    });
                }

                await dbTransaction.CommitAsync();

                return StatusCode(StatusCodes.Status200OK, new ControllerReturnResponse<List<DeviceTypeResponse>>()
                {
                    ResponseCode = HttpStatusCode.OK,
                    ResponseDescription = result.StatusMessage,
                    StatusCode = result.StatusCode,
                    StatusMessage = result.StatusMessage,
                    ObjectValue = result.ObjectValue
                });
            }
            else if (result.StatusCode == Utils.NotFound)
            {
                await dbTransaction.RollbackAsync();

                return StatusCode(StatusCodes.Status404NotFound, new ControllerReturnResponse<List<DeviceTypeResponse>>()
                {
                    ResponseCode = HttpStatusCode.NotFound,
                    ResponseDescription = result.StatusMessage,
                    StatusCode = result.StatusCode,
                    StatusMessage = result.StatusMessage
                });
            }
            else
            {
                await dbTransaction.RollbackAsync();

                return StatusCode(StatusCodes.Status400BadRequest, new ControllerReturnResponse<List<DeviceTypeResponse>>()
                {
                    ResponseCode = HttpStatusCode.BadRequest,
                    ResponseDescription = result.StatusMessage,
                    StatusCode = result.StatusCode,
                    StatusMessage = result.StatusMessage
                });
            }
        }
    }
}
