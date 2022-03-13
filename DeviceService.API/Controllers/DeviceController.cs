using AutoMapper;
using DeviceService.Core.Data.DataContext;
using DeviceService.Core.Dtos.AuditReport;
using DeviceService.Core.Dtos.Device;
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
using static DeviceService.Core.Helpers.Common.Utils;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DeviceService.API.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class DeviceController : ControllerBase
    {
        private readonly DeviceContext _deviceContext;
        private readonly IDeviceRepository _deviceRepository;
        private readonly IMapper _mapper;
        private readonly IAuditReportRepository _auditReportRepository;

        public DeviceController(DeviceContext deviceContext, IDeviceRepository deviceRepository, IMapper mapper, IAuditReportRepository auditReportRepository)
        {
            _deviceContext = deviceContext;
            _deviceRepository = deviceRepository;
            _mapper = mapper;
            _auditReportRepository = auditReportRepository;
        }

        // GET api/Device/1
        /// <summary>
        /// GET SINGLE DEVICE BY ID
        /// </summary>
        /// <param name="deviceId">Device Id</param>
        /// <returns>Device Details</returns>
        /// <response code="200">Returns Device Details</response>
        /// <response code="400">If Request is Bad</response>
        /// <response code="404">If an Object is not Found</response>
        /// <response code="500">If Unknown Error Occurs</response>
        [ProducesResponseType(typeof(ControllerReturnResponse<DeviceResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status500InternalServerError)]
        [Consumes("application/json")]
        [RequiredFunctionalityName("GetDevice")]
        [HttpGet("{deviceId}")]
        public async Task<ActionResult<ControllerReturnResponse<DeviceResponse>>> GetDevice([FromRoute][Required] int deviceId)
        {
            var result = await _deviceRepository.GetDevice(deviceId);

            //LOG THE ACTIONS
            LogWriter.WriteLog(result.Logs);

            if (result.StatusCode == Utils.Success)
            {
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "GetDevice",
                    AuditReportActivityResourceId = new List<int>() { result.ObjectValue.DeviceId }
                });

                if (auditResult.StatusCode != Utils.Success)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, new ControllerReturnResponse<DeviceResponse>()
                    {
                        ResponseCode = HttpStatusCode.BadRequest,
                        ResponseDescription = Utils.StatusMessageAuditReportError,
                        StatusCode = Utils.AuditReportError,
                        StatusMessage = Utils.StatusMessageAuditReportError
                    });
                }

                return StatusCode(StatusCodes.Status200OK, new ControllerReturnResponse<DeviceResponse>()
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
                return StatusCode(StatusCodes.Status404NotFound, new ControllerReturnResponse<DeviceResponse>()
                {
                    ResponseCode = HttpStatusCode.NotFound,
                    ResponseDescription = result.StatusMessage,
                    StatusCode = result.StatusCode,
                    StatusMessage = result.StatusMessage
                });
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ControllerReturnResponse<DeviceResponse>()
                {
                    ResponseCode = HttpStatusCode.BadRequest,
                    ResponseDescription = result.StatusMessage,
                    StatusCode = result.StatusCode,
                    StatusMessage = result.StatusMessage
                });
            }
        }

        // GET api/Device?PageNumber=1&PageSize=4
        /// <summary>
        /// GET ALL DEVICES
        /// </summary>
        /// <returns>Devices Details</returns>
        /// <response code="200">Returns Devices Details</response>
        /// <response code="400">If Request is Bad</response>
        /// <response code="404">If an Object is not Found</response>
        /// <response code="500">If Unknown Error Occurs</response>
        [ProducesResponseType(typeof(ControllerReturnResponse<List<DeviceResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status500InternalServerError)]
        [Consumes("application/json")]
        [RequiredFunctionalityName("GetDevices")]
        [HttpGet]
        public async Task<ActionResult<ControllerReturnResponse<List<DeviceResponse>>>> GetDevices([FromQuery] UserParams userParams)
        {
            var result = await _deviceRepository.GetDevices(userParams);

            //LOG THE ACTIONS
            LogWriter.WriteLog(result.Logs);

            if (result.StatusCode == Utils.Success)
            {
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "GetDevices",
                    AuditReportActivityResourceId = new List<int>() { }
                });

                if (auditResult.StatusCode != Utils.Success)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, new ControllerReturnResponse<List<DeviceResponse>>()
                    {
                        ResponseCode = HttpStatusCode.BadRequest,
                        ResponseDescription = Utils.StatusMessageAuditReportError,
                        StatusCode = Utils.AuditReportError,
                        StatusMessage = Utils.StatusMessageAuditReportError
                    });
                }

                return StatusCode(StatusCodes.Status200OK, new ControllerReturnResponse<List<DeviceResponse>>()
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
                return StatusCode(StatusCodes.Status404NotFound, new ControllerReturnResponse<List<DeviceResponse>>()
                {
                    ResponseCode = HttpStatusCode.NotFound,
                    ResponseDescription = result.StatusMessage,
                    StatusCode = result.StatusCode,
                    StatusMessage = result.StatusMessage
                });
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ControllerReturnResponse<List<DeviceResponse>>()
                {
                    ResponseCode = HttpStatusCode.BadRequest,
                    ResponseDescription = result.StatusMessage,
                    StatusCode = result.StatusCode,
                    StatusMessage = result.StatusMessage
                });
            }
        }

        // GET api/Device/User?PageNumber=1&PageSize=4
        /// <summary>
        /// GET ALL DEVICES FOR USER
        /// </summary>
        /// <returns>Devices Details</returns>
        /// <response code="200">Returns Devices Details</response>
        /// <response code="400">If Request is Bad</response>
        /// <response code="404">If an Object is not Found</response>
        /// <response code="500">If Unknown Error Occurs</response>
        [ProducesResponseType(typeof(ControllerReturnResponse<List<DeviceResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status500InternalServerError)]
        [Consumes("application/json")]
        [RequiredFunctionalityName("GetDevicesForUser")]
        [HttpGet("User")]
        public async Task<ActionResult<ControllerReturnResponse<List<DeviceResponse>>>> GetDevicesForUser([FromQuery] UserParams userParams)
        {
            var result = await _deviceRepository.GetDevicesForUser(userParams);

            //LOG THE ACTIONS
            LogWriter.WriteLog(result.Logs);

            if (result.StatusCode == Utils.Success)
            {
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "GetDevicesForUser",
                    AuditReportActivityResourceId = new List<int>() { }
                });

                if (auditResult.StatusCode != Utils.Success)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, new ControllerReturnResponse<List<DeviceResponse>>()
                    {
                        ResponseCode = HttpStatusCode.BadRequest,
                        ResponseDescription = Utils.StatusMessageAuditReportError,
                        StatusCode = Utils.AuditReportError,
                        StatusMessage = Utils.StatusMessageAuditReportError
                    });
                }

                return StatusCode(StatusCodes.Status200OK, new ControllerReturnResponse<List<DeviceResponse>>()
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
                return StatusCode(StatusCodes.Status404NotFound, new ControllerReturnResponse<List<DeviceResponse>>()
                {
                    ResponseCode = HttpStatusCode.NotFound,
                    ResponseDescription = result.StatusMessage,
                    StatusCode = result.StatusCode,
                    StatusMessage = result.StatusMessage
                });
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ControllerReturnResponse<List<DeviceResponse>>()
                {
                    ResponseCode = HttpStatusCode.BadRequest,
                    ResponseDescription = result.StatusMessage,
                    StatusCode = result.StatusCode,
                    StatusMessage = result.StatusMessage
                });
            }
        }

        // GET api/Device/Status/Offline?PageNumber=1&PageSize=4
        /// <summary>
        /// GET DEVICES BY STATUS
        /// </summary>
        /// <param name="statusId">DeviceStatus Id</param>
        /// <returns>Devices Details</returns>
        /// <response code="200">Returns Devices Details</response>
        /// <response code="400">If Request is Bad</response>
        /// <response code="404">If an Object is not Found</response>
        /// <response code="500">If Unknown Error Occurs</response>
        [ProducesResponseType(typeof(ControllerReturnResponse<List<DevicePartialResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status500InternalServerError)]
        [Consumes("application/json")]
        [RequiredFunctionalityName("GetDevicesByStatus")]
        [HttpGet("Status/{statusId}")]
        public async Task<ActionResult<ControllerReturnResponse<List<DevicePartialResponse>>>> GetDevicesByStatus([FromQuery] UserParams userParams, [FromRoute] DeviceStatus statusId)
        {
            var result = await _deviceRepository.GetDevicesByStatus(userParams, statusId);

            //LOG THE ACTIONS
            LogWriter.WriteLog(result.Logs);

            if (result.StatusCode == Utils.Success)
            {
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "GetDevicesByStatus",
                    AuditReportActivityResourceId = new List<int>() { }
                });

                if (auditResult.StatusCode != Utils.Success)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, new ControllerReturnResponse<List<DevicePartialResponse>>()
                    {
                        ResponseCode = HttpStatusCode.BadRequest,
                        ResponseDescription = Utils.StatusMessageAuditReportError,
                        StatusCode = Utils.AuditReportError,
                        StatusMessage = Utils.StatusMessageAuditReportError
                    });
                }

                return StatusCode(StatusCodes.Status200OK, new ControllerReturnResponse<List<DevicePartialResponse>>()
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
                return StatusCode(StatusCodes.Status404NotFound, new ControllerReturnResponse<List<DevicePartialResponse>>()
                {
                    ResponseCode = HttpStatusCode.NotFound,
                    ResponseDescription = result.StatusMessage,
                    StatusCode = result.StatusCode,
                    StatusMessage = result.StatusMessage
                });
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ControllerReturnResponse<List<DevicePartialResponse>>()
                {
                    ResponseCode = HttpStatusCode.BadRequest,
                    ResponseDescription = result.StatusMessage,
                    StatusCode = result.StatusCode,
                    StatusMessage = result.StatusMessage
                });
            }
        }

        // GET api/Device/Status/Offline/User?PageNumber=1&PageSize=4
        /// <summary>
        /// GET DEVICES BY STATUS FOR USER
        /// </summary>
        /// <param name="statusId">DeviceStatus Id</param>
        /// <returns>Devices Details</returns>
        /// <response code="200">Returns Devices Details</response>
        /// <response code="400">If Request is Bad</response>
        /// <response code="404">If an Object is not Found</response>
        /// <response code="500">If Unknown Error Occurs</response>
        [ProducesResponseType(typeof(ControllerReturnResponse<List<DevicePartialResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status500InternalServerError)]
        [Consumes("application/json")]
        [RequiredFunctionalityName("GetDevicesByStatusForUser")]
        [HttpGet("Status/{statusId}/User")]
        public async Task<ActionResult<ControllerReturnResponse<List<DevicePartialResponse>>>> GetDevicesByStatusForUser([FromQuery] UserParams userParams, [FromRoute] DeviceStatus statusId)
        {
            var result = await _deviceRepository.GetDevicesByStatusForUser(userParams, statusId);

            //LOG THE ACTIONS
            LogWriter.WriteLog(result.Logs);

            if (result.StatusCode == Utils.Success)
            {
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "GetDevicesByStatusForUser",
                    AuditReportActivityResourceId = new List<int>() { }
                });

                if (auditResult.StatusCode != Utils.Success)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, new ControllerReturnResponse<List<DevicePartialResponse>>()
                    {
                        ResponseCode = HttpStatusCode.BadRequest,
                        ResponseDescription = Utils.StatusMessageAuditReportError,
                        StatusCode = Utils.AuditReportError,
                        StatusMessage = Utils.StatusMessageAuditReportError
                    });
                }

                return StatusCode(StatusCodes.Status200OK, new ControllerReturnResponse<List<DevicePartialResponse>>()
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
                return StatusCode(StatusCodes.Status404NotFound, new ControllerReturnResponse<List<DevicePartialResponse>>()
                {
                    ResponseCode = HttpStatusCode.NotFound,
                    ResponseDescription = result.StatusMessage,
                    StatusCode = result.StatusCode,
                    StatusMessage = result.StatusMessage
                });
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ControllerReturnResponse<List<DevicePartialResponse>>()
                {
                    ResponseCode = HttpStatusCode.BadRequest,
                    ResponseDescription = result.StatusMessage,
                    StatusCode = result.StatusCode,
                    StatusMessage = result.StatusMessage
                });
            }
        }

        // GET api/Device/DeviceType/1?PageNumber=1&PageSize=4
        /// <summary>
        /// GET DEVICES BY DEVICE TYPE ID
        /// </summary>
        /// <param name="deviceTypeId">DeviceType Id</param>
        /// <returns>Devices Details</returns>
        /// <response code="200">Returns Devices Details</response>
        /// <response code="400">If Request is Bad</response>
        /// <response code="404">If an Object is not Found</response>
        /// <response code="500">If Unknown Error Occurs</response>
        [ProducesResponseType(typeof(ControllerReturnResponse<List<DevicePartialResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status500InternalServerError)]
        [Consumes("application/json")]
        [RequiredFunctionalityName("GetDevicesByDeviceType")]
        [HttpGet("DeviceType/{deviceTypeId}")]
        public async Task<ActionResult<ControllerReturnResponse<List<DevicePartialResponse>>>> GetDevicesByDeviceType([FromQuery] UserParams userParams, [FromRoute] int deviceTypeId)
        {
            var result = await _deviceRepository.GetDevicesByDeviceType(userParams, deviceTypeId);

            //LOG THE ACTIONS
            LogWriter.WriteLog(result.Logs);

            if (result.StatusCode == Utils.Success)
            {
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "GetDevicesByDeviceType",
                    AuditReportActivityResourceId = new List<int>() { }
                });

                if (auditResult.StatusCode != Utils.Success)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, new ControllerReturnResponse<List<DevicePartialResponse>>()
                    {
                        ResponseCode = HttpStatusCode.BadRequest,
                        ResponseDescription = Utils.StatusMessageAuditReportError,
                        StatusCode = Utils.AuditReportError,
                        StatusMessage = Utils.StatusMessageAuditReportError
                    });
                }

                return StatusCode(StatusCodes.Status200OK, new ControllerReturnResponse<List<DevicePartialResponse>>()
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
                return StatusCode(StatusCodes.Status404NotFound, new ControllerReturnResponse<List<DevicePartialResponse>>()
                {
                    ResponseCode = HttpStatusCode.NotFound,
                    ResponseDescription = result.StatusMessage,
                    StatusCode = result.StatusCode,
                    StatusMessage = result.StatusMessage
                });
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ControllerReturnResponse<List<DevicePartialResponse>>()
                {
                    ResponseCode = HttpStatusCode.BadRequest,
                    ResponseDescription = result.StatusMessage,
                    StatusCode = result.StatusCode,
                    StatusMessage = result.StatusMessage
                });
            }
        }

        // GET api/Device/DeviceType/1/User?PageNumber=1&PageSize=4
        /// <summary>
        /// GET DEVICES BY DEVICE TYPE FOR USER
        /// </summary>
        /// <param name="deviceTypeId">DeviceType Id</param>
        /// <returns>Devices Details</returns>
        /// <response code="200">Returns Devices Details</response>
        /// <response code="400">If Request is Bad</response>
        /// <response code="404">If an Object is not Found</response>
        /// <response code="500">If Unknown Error Occurs</response>
        [ProducesResponseType(typeof(ControllerReturnResponse<List<DevicePartialResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status500InternalServerError)]
        [Consumes("application/json")]
        [RequiredFunctionalityName("GetDevicesByDeviceTypeForUser")]
        [HttpGet("DeviceType/{deviceTypeId}/User")]
        public async Task<ActionResult<ControllerReturnResponse<List<DevicePartialResponse>>>> GetDevicesByDeviceTypeForUser([FromQuery] UserParams userParams, [FromRoute] int deviceTypeId)
        {
            var result = await _deviceRepository.GetDevicesByDeviceTypeForUser(userParams, deviceTypeId);

            //LOG THE ACTIONS
            LogWriter.WriteLog(result.Logs);

            if (result.StatusCode == Utils.Success)
            {
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "GetDevicesByDeviceTypeForUser",
                    AuditReportActivityResourceId = new List<int>() { }
                });

                if (auditResult.StatusCode != Utils.Success)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, new ControllerReturnResponse<List<DevicePartialResponse>>()
                    {
                        ResponseCode = HttpStatusCode.BadRequest,
                        ResponseDescription = Utils.StatusMessageAuditReportError,
                        StatusCode = Utils.AuditReportError,
                        StatusMessage = Utils.StatusMessageAuditReportError
                    });
                }

                return StatusCode(StatusCodes.Status200OK, new ControllerReturnResponse<List<DevicePartialResponse>>()
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
                return StatusCode(StatusCodes.Status404NotFound, new ControllerReturnResponse<List<DevicePartialResponse>>()
                {
                    ResponseCode = HttpStatusCode.NotFound,
                    ResponseDescription = result.StatusMessage,
                    StatusCode = result.StatusCode,
                    StatusMessage = result.StatusMessage
                });
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ControllerReturnResponse<List<DevicePartialResponse>>()
                {
                    ResponseCode = HttpStatusCode.BadRequest,
                    ResponseDescription = result.StatusMessage,
                    StatusCode = result.StatusCode,
                    StatusMessage = result.StatusMessage
                });
            }
        }

        // GET api/Device/1/RelatedDevices
        /// <summary>
        /// GET SINGLE DEVICE WITH ALL RELATED DEVICES BY DEVICETYPE
        /// </summary>
        /// <param name="deviceId">Device Id</param>
        /// <returns>Device Details</returns>
        /// <response code="200">Returns Device Details</response>
        /// <response code="400">If Request is Bad</response>
        /// <response code="404">If an Object is not Found</response>
        /// <response code="500">If Unknown Error Occurs</response>
        [ProducesResponseType(typeof(ControllerReturnResponse<DeviceWithRelatedDevicesResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status500InternalServerError)]
        [Consumes("application/json")]
        [RequiredFunctionalityName("GetDeviceWithRelatedDevices")]
        [HttpGet("{deviceId}/RelatedDevices")]
        public async Task<ActionResult<ControllerReturnResponse<DeviceWithRelatedDevicesResponse>>> GetDeviceWithRelatedDevices([FromRoute][Required] int deviceId)
        {
            var result = await _deviceRepository.GetDeviceWithRelatedDevices(deviceId);

            //LOG THE ACTIONS
            LogWriter.WriteLog(result.Logs);

            if (result.StatusCode == Utils.Success)
            {
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "GetDeviceWithRelatedDevices",
                    AuditReportActivityResourceId = new List<int>() { result.ObjectValue.DeviceId }
                });

                if (auditResult.StatusCode != Utils.Success)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, new ControllerReturnResponse<DeviceWithRelatedDevicesResponse>()
                    {
                        ResponseCode = HttpStatusCode.BadRequest,
                        ResponseDescription = Utils.StatusMessageAuditReportError,
                        StatusCode = Utils.AuditReportError,
                        StatusMessage = Utils.StatusMessageAuditReportError
                    });
                }

                return StatusCode(StatusCodes.Status200OK, new ControllerReturnResponse<DeviceWithRelatedDevicesResponse>()
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
                return StatusCode(StatusCodes.Status404NotFound, new ControllerReturnResponse<DeviceWithRelatedDevicesResponse>()
                {
                    ResponseCode = HttpStatusCode.NotFound,
                    ResponseDescription = result.StatusMessage,
                    StatusCode = result.StatusCode,
                    StatusMessage = result.StatusMessage
                });
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ControllerReturnResponse<DeviceWithRelatedDevicesResponse>()
                {
                    ResponseCode = HttpStatusCode.BadRequest,
                    ResponseDescription = result.StatusMessage,
                    StatusCode = result.StatusCode,
                    StatusMessage = result.StatusMessage
                });
            }
        }

        // PUT api/Device/1/Name
        /// <summary>
        /// UPDATE DEVICE NAME
        /// </summary>
        /// <param name="deviceId">Device Id</param>
        /// <param name="deviceToUpdate">Device Request Body</param>
        /// <returns>Device Details</returns>
        /// <response code="200">Returns Device Details</response>
        /// <response code="400">If Request is Bad</response>
        /// <response code="404">If an Object is not Found</response>
        /// <response code="500">If Unknown Error Occurs</response>
        [ProducesResponseType(typeof(ControllerReturnResponse<DevicePartialResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status500InternalServerError)]
        [Consumes("application/json")]
        [RequiredFunctionalityName("PutUpdateDeviceName")]
        [HttpPut("{deviceId}/Name")]
        public async Task<ActionResult<ControllerReturnResponse<DevicePartialResponse>>> PutUpdateDeviceName([FromRoute][Required] int deviceId, [FromBody] DeviceToUpdate deviceToUpdate)
        {
            var dbTransaction = await _deviceContext.Database.BeginTransactionAsync();

            var result = await _deviceRepository.UpdateDeviceName(deviceId, deviceToUpdate);

            //LOG THE ACTIONS
            LogWriter.WriteLog(result.Logs);

            if (result.StatusCode == Utils.Success)
            {
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "PutUpdateDeviceName",
                    AuditReportActivityResourceId = new List<int>() { result.ObjectValue.DeviceId }
                });

                if (auditResult.StatusCode != Utils.Success)
                {
                    await dbTransaction.RollbackAsync();

                    return StatusCode(StatusCodes.Status400BadRequest, new ControllerReturnResponse<DevicePartialResponse>()
                    {
                        ResponseCode = HttpStatusCode.BadRequest,
                        ResponseDescription = Utils.StatusMessageAuditReportError,
                        StatusCode = Utils.AuditReportError,
                        StatusMessage = Utils.StatusMessageAuditReportError
                    });
                }

                await dbTransaction.CommitAsync();

                return StatusCode(StatusCodes.Status200OK, new ControllerReturnResponse<DevicePartialResponse>()
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

                return StatusCode(StatusCodes.Status404NotFound, new ControllerReturnResponse<DevicePartialResponse>()
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

                return StatusCode(StatusCodes.Status400BadRequest, new ControllerReturnResponse<DevicePartialResponse>()
                {
                    ResponseCode = HttpStatusCode.BadRequest,
                    ResponseDescription = result.StatusMessage,
                    StatusCode = result.StatusCode,
                    StatusMessage = result.StatusMessage
                });
            }
        }

        // PUT api/Device/1/Status
        /// <summary>
        /// UPDATE DEVICE STATUS
        /// </summary>
        /// <param name="deviceId">Device Id</param>
        /// <param name="deviceToUpdate">Device Request Body</param>
        /// <returns>Device Details</returns>
        /// <response code="200">Returns Device Details</response>
        /// <response code="400">If Request is Bad</response>
        /// <response code="404">If an Object is not Found</response>
        /// <response code="500">If Unknown Error Occurs</response>
        [ProducesResponseType(typeof(ControllerReturnResponse<DevicePartialResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status500InternalServerError)]
        [Consumes("application/json")]
        [RequiredFunctionalityName("PutUpdateDeviceStatus")]
        [HttpPut("{deviceId}/Status")]
        public async Task<ActionResult<ControllerReturnResponse<DevicePartialResponse>>> PutUpdateDeviceStatus([FromRoute][Required] int deviceId, [FromBody] StatusUpdate deviceToUpdate)
        {
            var dbTransaction = await _deviceContext.Database.BeginTransactionAsync();

            var result = await _deviceRepository.UpdateDeviceStatus(deviceId, deviceToUpdate);

            //LOG THE ACTIONS
            LogWriter.WriteLog(result.Logs);

            if (result.StatusCode == Utils.Success)
            {
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "PutUpdateDeviceStatus",
                    AuditReportActivityResourceId = new List<int>() { result.ObjectValue.DeviceId }
                });

                if (auditResult.StatusCode != Utils.Success)
                {
                    await dbTransaction.RollbackAsync();

                    return StatusCode(StatusCodes.Status400BadRequest, new ControllerReturnResponse<DevicePartialResponse>()
                    {
                        ResponseCode = HttpStatusCode.BadRequest,
                        ResponseDescription = Utils.StatusMessageAuditReportError,
                        StatusCode = Utils.AuditReportError,
                        StatusMessage = Utils.StatusMessageAuditReportError
                    });
                }

                await dbTransaction.CommitAsync();

                return StatusCode(StatusCodes.Status200OK, new ControllerReturnResponse<DevicePartialResponse>()
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

                return StatusCode(StatusCodes.Status404NotFound, new ControllerReturnResponse<DevicePartialResponse>()
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

                return StatusCode(StatusCodes.Status400BadRequest, new ControllerReturnResponse<DevicePartialResponse>()
                {
                    ResponseCode = HttpStatusCode.BadRequest,
                    ResponseDescription = result.StatusMessage,
                    StatusCode = result.StatusCode,
                    StatusMessage = result.StatusMessage
                });
            }
        }

        // PUT api/Device/1/Temperature
        /// <summary>
        /// UPDATE DEVICE TEMPERATURE
        /// </summary>
        /// <param name="deviceId">Device Id</param>
        /// <param name="deviceToUpdate">Device Request Body</param>
        /// <returns>Device Details</returns>
        /// <response code="200">Returns Device Details</response>
        /// <response code="400">If Request is Bad</response>
        /// <response code="404">If an Object is not Found</response>
        /// <response code="500">If Unknown Error Occurs</response>
        [ProducesResponseType(typeof(ControllerReturnResponse<DevicePartialResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status500InternalServerError)]
        [Consumes("application/json")]
        [RequiredFunctionalityName("PutUpdateDeviceTemperature")]
        [HttpPut("{deviceId}/Temperature")]
        public async Task<ActionResult<ControllerReturnResponse<DevicePartialResponse>>> PutUpdateDeviceTemperature([FromRoute][Required] int deviceId, [FromBody] TemperatureUpdate deviceToUpdate)
        {
            var dbTransaction = await _deviceContext.Database.BeginTransactionAsync();

            var result = await _deviceRepository.UpdateDeviceTemperature(deviceId, deviceToUpdate);

            //LOG THE ACTIONS
            LogWriter.WriteLog(result.Logs);

            if (result.StatusCode == Utils.Success)
            {
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "PutUpdateDeviceTemperature",
                    AuditReportActivityResourceId = new List<int>() { result.ObjectValue.DeviceId }
                });

                if (auditResult.StatusCode != Utils.Success)
                {
                    await dbTransaction.RollbackAsync();

                    return StatusCode(StatusCodes.Status400BadRequest, new ControllerReturnResponse<DevicePartialResponse>()
                    {
                        ResponseCode = HttpStatusCode.BadRequest,
                        ResponseDescription = Utils.StatusMessageAuditReportError,
                        StatusCode = Utils.AuditReportError,
                        StatusMessage = Utils.StatusMessageAuditReportError
                    });
                }

                await dbTransaction.CommitAsync();

                return StatusCode(StatusCodes.Status200OK, new ControllerReturnResponse<DevicePartialResponse>()
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

                return StatusCode(StatusCodes.Status404NotFound, new ControllerReturnResponse<DevicePartialResponse>()
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

                return StatusCode(StatusCodes.Status400BadRequest, new ControllerReturnResponse<DevicePartialResponse>()
                {
                    ResponseCode = HttpStatusCode.BadRequest,
                    ResponseDescription = result.StatusMessage,
                    StatusCode = result.StatusCode,
                    StatusMessage = result.StatusMessage
                });
            }
        }

        // PUT api/Device/1/Usage
        /// <summary>
        /// UPDATE DEVICE USAGE
        /// </summary>
        /// <param name="deviceId">Device Id</param>
        /// <param name="deviceToUpdate">Device Request Body</param>
        /// <returns>Device Details</returns>
        /// <response code="200">Returns Device Details</response>
        /// <response code="400">If Request is Bad</response>
        /// <response code="404">If an Object is not Found</response>
        /// <response code="500">If Unknown Error Occurs</response>
        [ProducesResponseType(typeof(ControllerReturnResponse<DevicePartialResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status500InternalServerError)]
        [Consumes("application/json")]
        [RequiredFunctionalityName("PutUpdateDeviceUsage")]
        [HttpPut("{deviceId}/Usage")]
        public async Task<ActionResult<ControllerReturnResponse<DevicePartialResponse>>> PutUpdateDeviceUsage([FromRoute][Required] int deviceId, [FromBody] UsageUpdate deviceToUpdate)
        {
            var dbTransaction = await _deviceContext.Database.BeginTransactionAsync();

            var result = await _deviceRepository.UpdateDeviceUsageHours(deviceId, deviceToUpdate);

            //LOG THE ACTIONS
            LogWriter.WriteLog(result.Logs);

            if (result.StatusCode == Utils.Success)
            {
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "PutUpdateDeviceUsage",
                    AuditReportActivityResourceId = new List<int>() { result.ObjectValue.DeviceId }
                });

                if (auditResult.StatusCode != Utils.Success)
                {
                    await dbTransaction.RollbackAsync();

                    return StatusCode(StatusCodes.Status400BadRequest, new ControllerReturnResponse<DevicePartialResponse>()
                    {
                        ResponseCode = HttpStatusCode.BadRequest,
                        ResponseDescription = Utils.StatusMessageAuditReportError,
                        StatusCode = Utils.AuditReportError,
                        StatusMessage = Utils.StatusMessageAuditReportError
                    });
                }

                await dbTransaction.CommitAsync();

                return StatusCode(StatusCodes.Status200OK, new ControllerReturnResponse<DevicePartialResponse>()
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

                return StatusCode(StatusCodes.Status404NotFound, new ControllerReturnResponse<DevicePartialResponse>()
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

                return StatusCode(StatusCodes.Status400BadRequest, new ControllerReturnResponse<DevicePartialResponse>()
                {
                    ResponseCode = HttpStatusCode.BadRequest,
                    ResponseDescription = result.StatusMessage,
                    StatusCode = result.StatusCode,
                    StatusMessage = result.StatusMessage
                });
            }
        }

        // POST api/Device
        /// <summary>
        /// CREATE DEVICE
        /// </summary>
        /// <param name="deviceRequest">Device Request Body</param>
        /// <returns>Device Details</returns>
        /// <response code="200">Returns Device Details</response>
        /// <response code="400">If Request is Bad</response>
        /// <response code="404">If an Object is not Found</response>
        /// <response code="500">If Unknown Error Occurs</response>
        [ProducesResponseType(typeof(ControllerReturnResponse<DeviceResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status500InternalServerError)]
        //[Consumes("application/json")]
        [RequiredFunctionalityName("PostDevice")]
        [HttpPost]
        public async Task<ActionResult<ControllerReturnResponse<DeviceResponse>>> PostDevice([FromForm] DeviceRequest deviceRequest)
        {
            var dbTransaction = await _deviceContext.Database.BeginTransactionAsync();

            var result = await _deviceRepository.CreateDevice(deviceRequest);

            //LOG THE ACTIONS
            LogWriter.WriteLog(result.Logs);

            if (result.StatusCode == Utils.Success)
            {
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "PostDevice",
                    AuditReportActivityResourceId = new List<int>() { result.ObjectValue.DeviceId }
                });

                if (auditResult.StatusCode != Utils.Success)
                {
                    await dbTransaction.RollbackAsync();

                    return StatusCode(StatusCodes.Status400BadRequest, new ControllerReturnResponse<DeviceResponse>()
                    {
                        ResponseCode = HttpStatusCode.BadRequest,
                        ResponseDescription = Utils.StatusMessageAuditReportError,
                        StatusCode = Utils.AuditReportError,
                        StatusMessage = Utils.StatusMessageAuditReportError
                    });
                }

                await dbTransaction.CommitAsync();

                return StatusCode(StatusCodes.Status200OK, new ControllerReturnResponse<DeviceResponse>()
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

                return StatusCode(StatusCodes.Status404NotFound, new ControllerReturnResponse<DeviceResponse>()
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

                return StatusCode(StatusCodes.Status400BadRequest, new ControllerReturnResponse<DeviceResponse>()
                {
                    ResponseCode = HttpStatusCode.BadRequest,
                    ResponseDescription = result.StatusMessage,
                    StatusCode = result.StatusCode,
                    StatusMessage = result.StatusMessage
                });
            }
        }

        // POST api/Device/Delete
        /// <summary>
        /// DELETE DEVICES
        /// </summary>
        /// <param name="deviceIds">Devices Ids to Delete</param>
        /// <returns>Devices Details</returns>
        /// <response code="200">Returns Devices Details</response>
        /// <response code="400">If Request is Bad</response>
        /// <response code="404">If an Object is not Found</response>
        /// <response code="500">If Unknown Error Occurs</response>
        [ProducesResponseType(typeof(ControllerReturnResponse<List<DevicePartialResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status500InternalServerError)]
        [Consumes("application/json")]
        [RequiredFunctionalityName("PostDeleteDevice")]
        [HttpPost("Delete")]
        public async Task<ActionResult<ControllerReturnResponse<List<DevicePartialResponse>>>> PostDeleteDevice([FromBody] List<int> deviceIds)
        {
            var dbTransaction = await _deviceContext.Database.BeginTransactionAsync();

            var result = await _deviceRepository.DeleteDevice(deviceIds);

            //LOG THE ACTIONS
            LogWriter.WriteLog(result.Logs);

            if (result.StatusCode == Utils.Success)
            {
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "PostDeleteDevice",
                    AuditReportActivityResourceId = deviceIds
                });

                if (auditResult.StatusCode != Utils.Success)
                {
                    await dbTransaction.RollbackAsync();

                    return StatusCode(StatusCodes.Status400BadRequest, new ControllerReturnResponse<List<DevicePartialResponse>>()
                    {
                        ResponseCode = HttpStatusCode.BadRequest,
                        ResponseDescription = Utils.StatusMessageAuditReportError,
                        StatusCode = Utils.AuditReportError,
                        StatusMessage = Utils.StatusMessageAuditReportError
                    });
                }

                await dbTransaction.CommitAsync();

                return StatusCode(StatusCodes.Status200OK, new ControllerReturnResponse<List<DevicePartialResponse>>()
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

                return StatusCode(StatusCodes.Status404NotFound, new ControllerReturnResponse<List<DevicePartialResponse>>()
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

                return StatusCode(StatusCodes.Status400BadRequest, new ControllerReturnResponse<List<DevicePartialResponse>>()
                {
                    ResponseCode = HttpStatusCode.BadRequest,
                    ResponseDescription = result.StatusMessage,
                    StatusCode = result.StatusCode,
                    StatusMessage = result.StatusMessage
                });
            }
        }

        // POST api/Device/Search?PageNumber=1&PageSize=4
        /// <summary>
        /// SEARCH USER DEVICES BY NAME, STATUS AND DEVICE TYPE NAME
        /// </summary>
        /// <param name="deviceSearchRequest">DeviceSearch Request Body</param>
        /// <returns>Devices Details</returns>
        /// <response code="200">Returns Devices Details</response>
        /// <response code="400">If Request is Bad</response>
        /// <response code="404">If an Object is not Found</response>
        /// <response code="500">If Unknown Error Occurs</response>
        [ProducesResponseType(typeof(ControllerReturnResponse<List<DeviceResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status500InternalServerError)]
        [Consumes("application/json")]
        [RequiredFunctionalityName("PostSearchUserDevices")]
        [HttpPost("Search")]
        public async Task<ActionResult<ControllerReturnResponse<List<DeviceResponse>>>> PostSearchUserDevices([FromQuery] UserParams userParams, [FromBody] DeviceSearchRequest deviceSearchRequest)
        {
            var result = await _deviceRepository.SearchUserDevice(deviceSearchRequest, userParams);

            //LOG THE ACTIONS
            LogWriter.WriteLog(result.Logs);

            if (result.StatusCode == Utils.Success)
            {
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "PostSearchUserDevices",
                    AuditReportActivityResourceId = new List<int>() { }
                });

                if (auditResult.StatusCode != Utils.Success)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, new ControllerReturnResponse<List<DeviceResponse>>()
                    {
                        ResponseCode = HttpStatusCode.BadRequest,
                        ResponseDescription = Utils.StatusMessageAuditReportError,
                        StatusCode = Utils.AuditReportError,
                        StatusMessage = Utils.StatusMessageAuditReportError
                    });
                }

                return StatusCode(StatusCodes.Status200OK, new ControllerReturnResponse<List<DeviceResponse>>()
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
                return StatusCode(StatusCodes.Status404NotFound, new ControllerReturnResponse<List<DeviceResponse>>()
                {
                    ResponseCode = HttpStatusCode.NotFound,
                    ResponseDescription = result.StatusMessage,
                    StatusCode = result.StatusCode,
                    StatusMessage = result.StatusMessage
                });
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ControllerReturnResponse<List<DeviceResponse>>()
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
