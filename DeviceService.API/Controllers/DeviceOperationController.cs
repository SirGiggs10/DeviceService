using AutoMapper;
using DeviceService.Core.Data.DataContext;
using DeviceService.Core.Dtos.AuditReport;
using DeviceService.Core.Dtos.DeviceOperation;
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
    public class DeviceOperationController : ControllerBase
    {
        private readonly DeviceContext _deviceContext;
        private readonly IDeviceOperationRepository _deviceOperationRepository;
        private readonly IMapper _mapper;
        private readonly IAuditReportRepository _auditReportRepository;

        public DeviceOperationController(DeviceContext deviceContext, IDeviceOperationRepository deviceOperationRepository, IMapper mapper, IAuditReportRepository auditReportRepository)
        {
            _deviceContext = deviceContext;
            _deviceOperationRepository = deviceOperationRepository;
            _mapper = mapper;
            _auditReportRepository = auditReportRepository;
        }

        // GET api/DeviceOperation/1
        /// <summary>
        /// GET SINGLE DEVICE OPERATION BY ID
        /// </summary>
        /// <param name="deviceOperationId">Device Operation Id</param>
        /// <returns>Device Operation Details</returns>
        /// <response code="200">Returns Device Operation Details</response>
        /// <response code="400">If Request is Bad</response>
        /// <response code="404">If an Object is not Found</response>
        /// <response code="500">If Unknown Error Occurs</response>
        [ProducesResponseType(typeof(ControllerReturnResponse<DeviceOperationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status500InternalServerError)]
        [Consumes("application/json")]
        [RequiredFunctionalityName("GetDeviceOperation")]
        [HttpGet("{deviceOperationId}")]
        public async Task<ActionResult<ControllerReturnResponse<DeviceOperationResponse>>> GetDeviceOperation([FromRoute] [Required] int deviceOperationId)
        {
            var result = await _deviceOperationRepository.GetDeviceOperation(deviceOperationId);

            //LOG THE ACTIONS
            LogWriter.WriteLog(result.Logs);

            if (result.StatusCode == Utils.Success)
            {
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "GetDeviceOperation",
                    AuditReportActivityResourceId = new List<int>() { result.ObjectValue.DeviceOperationId }
                });

                if (auditResult.StatusCode != Utils.Success)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, new ControllerReturnResponse<DeviceOperationResponse>()
                    {
                        ResponseCode = HttpStatusCode.BadRequest,
                        ResponseDescription = Utils.StatusMessageAuditReportError,
                        StatusCode = Utils.AuditReportError,
                        StatusMessage = Utils.StatusMessageAuditReportError
                    });
                }

                return StatusCode(StatusCodes.Status200OK, new ControllerReturnResponse<DeviceOperationResponse>()
                {
                    ResponseCode = HttpStatusCode.OK,
                    ResponseDescription = result.StatusMessage,
                    StatusCode = result.StatusCode,
                    StatusMessage = result.StatusMessage,
                    ObjectValue = result.ObjectValue
                });
            }
            else if(result.StatusCode == Utils.NotFound)
            {
                return StatusCode(StatusCodes.Status404NotFound, new ControllerReturnResponse<DeviceOperationResponse>()
                {
                    ResponseCode = HttpStatusCode.NotFound,
                    ResponseDescription = result.StatusMessage,
                    StatusCode = result.StatusCode,
                    StatusMessage = result.StatusMessage
                });
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ControllerReturnResponse<DeviceOperationResponse>()
                {
                    ResponseCode = HttpStatusCode.BadRequest,
                    ResponseDescription = result.StatusMessage,
                    StatusCode = result.StatusCode,
                    StatusMessage = result.StatusMessage
                });
            }
        }

        // GET api/DeviceOperation?PageNumber=1&PageSize=4
        /// <summary>
        /// GET ALL DEVICE OPERATIONS
        /// </summary>
        /// <param name="deviceOperationId">Device Operation Id</param>
        /// <returns>Device Operations Details</returns>
        /// <response code="200">Returns Device Operations Details</response>
        /// <response code="400">If Request is Bad</response>
        /// <response code="404">If an Object is not Found</response>
        /// <response code="500">If Unknown Error Occurs</response>
        [ProducesResponseType(typeof(ControllerReturnResponse<List<DeviceOperationResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status500InternalServerError)]
        [Consumes("application/json")]
        [RequiredFunctionalityName("GetDeviceOperations")]
        [HttpGet]
        public async Task<ActionResult<ControllerReturnResponse<List<DeviceOperationResponse>>>> GetDeviceOperations([FromQuery] UserParams userParams)
        {
            var result = await _deviceOperationRepository.GetDeviceOperations(userParams);

            //LOG THE ACTIONS
            LogWriter.WriteLog(result.Logs);

            if (result.StatusCode == Utils.Success)
            {
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "GetDeviceOperations",
                    AuditReportActivityResourceId = new List<int>() { }
                });

                if (auditResult.StatusCode != Utils.Success)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, new ControllerReturnResponse<List<DeviceOperationResponse>>()
                    {
                        ResponseCode = HttpStatusCode.BadRequest,
                        ResponseDescription = Utils.StatusMessageAuditReportError,
                        StatusCode = Utils.AuditReportError,
                        StatusMessage = Utils.StatusMessageAuditReportError
                    });
                }

                return StatusCode(StatusCodes.Status200OK, new ControllerReturnResponse<List<DeviceOperationResponse>>()
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
                return StatusCode(StatusCodes.Status404NotFound, new ControllerReturnResponse<List<DeviceOperationResponse>>()
                {
                    ResponseCode = HttpStatusCode.NotFound,
                    ResponseDescription = result.StatusMessage,
                    StatusCode = result.StatusCode,
                    StatusMessage = result.StatusMessage
                });
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ControllerReturnResponse<List<DeviceOperationResponse>>()
                {
                    ResponseCode = HttpStatusCode.BadRequest,
                    ResponseDescription = result.StatusMessage,
                    StatusCode = result.StatusCode,
                    StatusMessage = result.StatusMessage
                });
            }
        }

        // PUT api/DeviceOperation/1
        /// <summary>
        /// UPDATE DEVICE OPERATION
        /// </summary>
        /// <param name="deviceOperationId">Device Operation Id</param>
        /// <param name="deviceOperationToUpdate">Device Operation Request Body</param>
        /// <returns>Device Operation Details</returns>
        /// <response code="200">Returns Device Operation Details</response>
        /// <response code="400">If Request is Bad</response>
        /// <response code="404">If an Object is not Found</response>
        /// <response code="500">If Unknown Error Occurs</response>
        [ProducesResponseType(typeof(ControllerReturnResponse<DeviceOperationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status500InternalServerError)]
        [Consumes("application/json")]
        [RequiredFunctionalityName("PutDeviceOperation")]
        [HttpPut("{deviceOperationId}")]
        public async Task<ActionResult<ControllerReturnResponse<DeviceOperationResponse>>> PutDeviceOperation([FromRoute][Required] int deviceOperationId, [FromBody] DeviceOperationToUpdate deviceOperationToUpdate)
        {
            var dbTransaction = await _deviceContext.Database.BeginTransactionAsync();

            var result = await _deviceOperationRepository.UpdateDeviceOperation(deviceOperationId, deviceOperationToUpdate);

            //LOG THE ACTIONS
            LogWriter.WriteLog(result.Logs);

            if (result.StatusCode == Utils.Success)
            {
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "PutDeviceOperation",
                    AuditReportActivityResourceId = new List<int>() { result.ObjectValue.DeviceOperationId }
                });

                if (auditResult.StatusCode != Utils.Success)
                {
                    await dbTransaction.RollbackAsync();

                    return StatusCode(StatusCodes.Status400BadRequest, new ControllerReturnResponse<DeviceOperationResponse>()
                    {
                        ResponseCode = HttpStatusCode.BadRequest,
                        ResponseDescription = Utils.StatusMessageAuditReportError,
                        StatusCode = Utils.AuditReportError,
                        StatusMessage = Utils.StatusMessageAuditReportError
                    });
                }

                await dbTransaction.CommitAsync();

                return StatusCode(StatusCodes.Status200OK, new ControllerReturnResponse<DeviceOperationResponse>()
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

                return StatusCode(StatusCodes.Status404NotFound, new ControllerReturnResponse<DeviceOperationResponse>()
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

                return StatusCode(StatusCodes.Status400BadRequest, new ControllerReturnResponse<DeviceOperationResponse>()
                {
                    ResponseCode = HttpStatusCode.BadRequest,
                    ResponseDescription = result.StatusMessage,
                    StatusCode = result.StatusCode,
                    StatusMessage = result.StatusMessage
                });
            }
        }

        // POST api/DeviceOperation
        /// <summary>
        /// CREATE DEVICE OPERATION
        /// </summary>
        /// <param name="deviceOperationRequest">Device Operation Request Body</param>
        /// <returns>Device Operation Details</returns>
        /// <response code="200">Returns Device Operation Details</response>
        /// <response code="400">If Request is Bad</response>
        /// <response code="404">If an Object is not Found</response>
        /// <response code="500">If Unknown Error Occurs</response>
        [ProducesResponseType(typeof(ControllerReturnResponse<DeviceOperationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status500InternalServerError)]
        [Consumes("application/json")]
        [RequiredFunctionalityName("PostDeviceOperation")]
        [HttpPost]
        public async Task<ActionResult<ControllerReturnResponse<DeviceOperationResponse>>> PostDeviceOperation([FromBody] DeviceOperationRequest deviceOperationRequest)
        {
            var dbTransaction = await _deviceContext.Database.BeginTransactionAsync();

            var result = await _deviceOperationRepository.CreateDeviceOperation(deviceOperationRequest);

            //LOG THE ACTIONS
            LogWriter.WriteLog(result.Logs);

            if (result.StatusCode == Utils.Success)
            {
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "PostDeviceOperation",
                    AuditReportActivityResourceId = new List<int>() { result.ObjectValue.DeviceOperationId }
                });

                if (auditResult.StatusCode != Utils.Success)
                {
                    await dbTransaction.RollbackAsync();

                    return StatusCode(StatusCodes.Status400BadRequest, new ControllerReturnResponse<DeviceOperationResponse>()
                    {
                        ResponseCode = HttpStatusCode.BadRequest,
                        ResponseDescription = Utils.StatusMessageAuditReportError,
                        StatusCode = Utils.AuditReportError,
                        StatusMessage = Utils.StatusMessageAuditReportError
                    });
                }

                await dbTransaction.CommitAsync();

                return StatusCode(StatusCodes.Status200OK, new ControllerReturnResponse<DeviceOperationResponse>()
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

                return StatusCode(StatusCodes.Status404NotFound, new ControllerReturnResponse<DeviceOperationResponse>()
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

                return StatusCode(StatusCodes.Status400BadRequest, new ControllerReturnResponse<DeviceOperationResponse>()
                {
                    ResponseCode = HttpStatusCode.BadRequest,
                    ResponseDescription = result.StatusMessage,
                    StatusCode = result.StatusCode,
                    StatusMessage = result.StatusMessage
                });
            }
        }

        // POST api/DeviceOperation/Delete
        /// <summary>
        /// DELETE DEVICE OPERATIONS
        /// </summary>
        /// <param name="deviceOperationIds">Device Operations Ids to Delete</param>
        /// <returns>Device Operations Details</returns>
        /// <response code="200">Returns Device Operations Details</response>
        /// <response code="400">If Request is Bad</response>
        /// <response code="404">If an Object is not Found</response>
        /// <response code="500">If Unknown Error Occurs</response>
        [ProducesResponseType(typeof(ControllerReturnResponse<List<DeviceOperationResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status500InternalServerError)]
        [Consumes("application/json")]
        [RequiredFunctionalityName("PostDeleteDeviceOperation")]
        [HttpPost("Delete")]
        public async Task<ActionResult<ControllerReturnResponse<List<DeviceOperationResponse>>>> PostDeleteDeviceOperation([FromBody] List<int> deviceOperationIds)
        {
            var dbTransaction = await _deviceContext.Database.BeginTransactionAsync();

            var result = await _deviceOperationRepository.DeleteDeviceOperation(deviceOperationIds);

            //LOG THE ACTIONS
            LogWriter.WriteLog(result.Logs);

            if (result.StatusCode == Utils.Success)
            {
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "PostDeleteDeviceOperation",
                    AuditReportActivityResourceId = deviceOperationIds
                });

                if (auditResult.StatusCode != Utils.Success)
                {
                    await dbTransaction.RollbackAsync();

                    return StatusCode(StatusCodes.Status400BadRequest, new ControllerReturnResponse<List<DeviceOperationResponse>>()
                    {
                        ResponseCode = HttpStatusCode.BadRequest,
                        ResponseDescription = Utils.StatusMessageAuditReportError,
                        StatusCode = Utils.AuditReportError,
                        StatusMessage = Utils.StatusMessageAuditReportError
                    });
                }

                await dbTransaction.CommitAsync();

                return StatusCode(StatusCodes.Status200OK, new ControllerReturnResponse<List<DeviceOperationResponse>>()
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

                return StatusCode(StatusCodes.Status404NotFound, new ControllerReturnResponse<List<DeviceOperationResponse>>()
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

                return StatusCode(StatusCodes.Status400BadRequest, new ControllerReturnResponse<List<DeviceOperationResponse>>()
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
