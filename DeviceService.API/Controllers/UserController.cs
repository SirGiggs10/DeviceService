using AutoMapper;
using DeviceService.Core.Data.DataContext;
using DeviceService.Core.Dtos.AuditReport;
using DeviceService.Core.Dtos.User;
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
using Microsoft.AspNetCore.Authorization;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DeviceService.API.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly DeviceContext _deviceContext;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IAuditReportRepository _auditReportRepository;

        public UserController(DeviceContext deviceContext, IUserRepository userRepository, IMapper mapper, IAuditReportRepository auditReportRepository)
        {
            _deviceContext = deviceContext;
            _userRepository = userRepository;
            _mapper = mapper;
            _auditReportRepository = auditReportRepository;
        }

        // GET api/User/1
        /// <summary>
        /// GET SINGLE USER BY ID
        /// </summary>
        /// <param name="userId">User Id</param>
        /// <returns>User Details</returns>
        /// <response code="200">Returns User Details</response>
        /// <response code="400">If Request is Bad</response>
        /// <response code="404">If an Object is not Found</response>
        /// <response code="500">If Unknown Error Occurs</response>
        [ProducesResponseType(typeof(ControllerReturnResponse<UserResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status500InternalServerError)]
        [Consumes("application/json")]
        [RequiredFunctionalityName("GetUser")]
        [HttpGet("{userId}")]
        public async Task<ActionResult<ControllerReturnResponse<UserResponse>>> GetUser([FromRoute][Required] int userId)
        {
            var result = await _userRepository.GetUser(userId);

            //LOG THE ACTIONS
            LogWriter.WriteLog(result.Logs);

            if (result.StatusCode == Utils.Success)
            {
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "GetUser",
                    AuditReportActivityResourceId = new List<int>() { result.ObjectValue.Id }
                });

                if (auditResult.StatusCode != Utils.Success)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, new ControllerReturnResponse<UserResponse>()
                    {
                        ResponseCode = HttpStatusCode.BadRequest,
                        ResponseDescription = Utils.StatusMessageAuditReportError,
                        StatusCode = Utils.AuditReportError,
                        StatusMessage = Utils.StatusMessageAuditReportError
                    });
                }

                return StatusCode(StatusCodes.Status200OK, new ControllerReturnResponse<UserResponse>()
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
                return StatusCode(StatusCodes.Status404NotFound, new ControllerReturnResponse<UserResponse>()
                {
                    ResponseCode = HttpStatusCode.NotFound,
                    ResponseDescription = result.StatusMessage,
                    StatusCode = result.StatusCode,
                    StatusMessage = result.StatusMessage
                });
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ControllerReturnResponse<UserResponse>()
                {
                    ResponseCode = HttpStatusCode.BadRequest,
                    ResponseDescription = result.StatusMessage,
                    StatusCode = result.StatusCode,
                    StatusMessage = result.StatusMessage
                });
            }
        }

        // GET api/User?PageNumber=1&PageSize=4
        /// <summary>
        /// GET ALL USERS
        /// </summary>
        /// <param name="userId">User Id</param>
        /// <returns>Users Details</returns>
        /// <response code="200">Returns Users Details</response>
        /// <response code="400">If Request is Bad</response>
        /// <response code="404">If an Object is not Found</response>
        /// <response code="500">If Unknown Error Occurs</response>
        [ProducesResponseType(typeof(ControllerReturnResponse<List<UserResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status500InternalServerError)]
        [Consumes("application/json")]
        [RequiredFunctionalityName("GetUsers")]
        [HttpGet]
        public async Task<ActionResult<ControllerReturnResponse<List<UserResponse>>>> GetUsers([FromQuery] UserParams userParams)
        {
            var result = await _userRepository.GetUsers(userParams);

            //LOG THE ACTIONS
            LogWriter.WriteLog(result.Logs);

            if (result.StatusCode == Utils.Success)
            {
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "GetUsers",
                    AuditReportActivityResourceId = new List<int>() { }
                });

                if (auditResult.StatusCode != Utils.Success)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, new ControllerReturnResponse<List<UserResponse>>()
                    {
                        ResponseCode = HttpStatusCode.BadRequest,
                        ResponseDescription = Utils.StatusMessageAuditReportError,
                        StatusCode = Utils.AuditReportError,
                        StatusMessage = Utils.StatusMessageAuditReportError
                    });
                }

                return StatusCode(StatusCodes.Status200OK, new ControllerReturnResponse<List<UserResponse>>()
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
                return StatusCode(StatusCodes.Status404NotFound, new ControllerReturnResponse<List<UserResponse>>()
                {
                    ResponseCode = HttpStatusCode.NotFound,
                    ResponseDescription = result.StatusMessage,
                    StatusCode = result.StatusCode,
                    StatusMessage = result.StatusMessage
                });
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ControllerReturnResponse<List<UserResponse>>()
                {
                    ResponseCode = HttpStatusCode.BadRequest,
                    ResponseDescription = result.StatusMessage,
                    StatusCode = result.StatusCode,
                    StatusMessage = result.StatusMessage
                });
            }
        }

        // PUT api/User/1
        /// <summary>
        /// UPDATE USER
        /// </summary>
        /// <param name="userId">User Id</param>
        /// <param name="userToUpdate">User Request Body</param>
        /// <returns>User Details</returns>
        /// <response code="200">Returns User Details</response>
        /// <response code="400">If Request is Bad</response>
        /// <response code="404">If an Object is not Found</response>
        /// <response code="500">If Unknown Error Occurs</response>
        [ProducesResponseType(typeof(ControllerReturnResponse<UserResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status500InternalServerError)]
        [Consumes("application/json")]
        [RequiredFunctionalityName("PutUser")]
        [HttpPut("{userId}")]
        public async Task<ActionResult<ControllerReturnResponse<UserResponse>>> PutUser([FromRoute][Required] int userId, [FromBody] UserToUpdate userToUpdate)
        {
            var dbTransaction = await _deviceContext.Database.BeginTransactionAsync();

            var result = await _userRepository.UpdateUser(userId, userToUpdate);

            //LOG THE ACTIONS
            LogWriter.WriteLog(result.Logs);

            if (result.StatusCode == Utils.Success)
            {
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "PutUser",
                    AuditReportActivityResourceId = new List<int>() { result.ObjectValue.Id }
                });

                if (auditResult.StatusCode != Utils.Success)
                {
                    await dbTransaction.RollbackAsync();

                    return StatusCode(StatusCodes.Status400BadRequest, new ControllerReturnResponse<UserResponse>()
                    {
                        ResponseCode = HttpStatusCode.BadRequest,
                        ResponseDescription = Utils.StatusMessageAuditReportError,
                        StatusCode = Utils.AuditReportError,
                        StatusMessage = Utils.StatusMessageAuditReportError
                    });
                }

                await dbTransaction.CommitAsync();

                return StatusCode(StatusCodes.Status200OK, new ControllerReturnResponse<UserResponse>()
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

                return StatusCode(StatusCodes.Status404NotFound, new ControllerReturnResponse<UserResponse>()
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

                return StatusCode(StatusCodes.Status400BadRequest, new ControllerReturnResponse<UserResponse>()
                {
                    ResponseCode = HttpStatusCode.BadRequest,
                    ResponseDescription = result.StatusMessage,
                    StatusCode = result.StatusCode,
                    StatusMessage = result.StatusMessage
                });
            }
        }

        // POST api/User
        /// <summary>
        /// CREATE USER
        /// </summary>
        /// <param name="userRequest">User Request Body</param>
        /// <returns>User Details</returns>
        /// <response code="200">Returns User Details</response>
        /// <response code="400">If Request is Bad</response>
        /// <response code="404">If an Object is not Found</response>
        /// <response code="500">If Unknown Error Occurs</response>
        [ProducesResponseType(typeof(ControllerReturnResponse<UserResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status500InternalServerError)]
        [Consumes("application/json")]
        [AllowAnonymous]
        //[RequiredFunctionalityName("PostUser")]
        [HttpPost]
        public async Task<ActionResult<ControllerReturnResponse<UserResponse>>> PostUser([FromBody] UserRequest userRequest)
        {
            var dbTransaction = await _deviceContext.Database.BeginTransactionAsync();

            var result = await _userRepository.CreateUser(userRequest);

            //LOG THE ACTIONS
            LogWriter.WriteLog(result.Logs);

            if (result.StatusCode == Utils.Success)
            {
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "PostUser",
                    AuditReportActivityResourceId = new List<int>() { result.ObjectValue.Id },
                    UserId = result.ObjectValue.Id
                });

                if (auditResult.StatusCode != Utils.Success)
                {
                    await dbTransaction.RollbackAsync();

                    return StatusCode(StatusCodes.Status400BadRequest, new ControllerReturnResponse<UserResponse>()
                    {
                        ResponseCode = HttpStatusCode.BadRequest,
                        ResponseDescription = Utils.StatusMessageAuditReportError,
                        StatusCode = Utils.AuditReportError,
                        StatusMessage = Utils.StatusMessageAuditReportError
                    });
                }

                await dbTransaction.CommitAsync();

                return StatusCode(StatusCodes.Status200OK, new ControllerReturnResponse<UserResponse>()
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

                return StatusCode(StatusCodes.Status404NotFound, new ControllerReturnResponse<UserResponse>()
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

                return StatusCode(StatusCodes.Status400BadRequest, new ControllerReturnResponse<UserResponse>()
                {
                    ResponseCode = HttpStatusCode.BadRequest,
                    ResponseDescription = result.StatusMessage,
                    StatusCode = result.StatusCode,
                    StatusMessage = result.StatusMessage
                });
            }
        }

        // DELETE api/User/1
        /// <summary>
        /// DELETE USERS
        /// </summary>
        /// <param name="userId">User Id to Delete</param>
        /// <returns>User Detail</returns>
        /// <response code="200">Returns User Detail</response>
        /// <response code="400">If Request is Bad</response>
        /// <response code="404">If an Object is not Found</response>
        /// <response code="500">If Unknown Error Occurs</response>
        [ProducesResponseType(typeof(ControllerReturnResponse<UserResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status500InternalServerError)]
        [Consumes("application/json")]
        [RequiredFunctionalityName("PostDeleteUser")]
        [HttpDelete("{userId}")]
        public async Task<ActionResult<ControllerReturnResponse<UserResponse>>> PostDeleteUser([FromRoute][Required] int userId)
        {
            var dbTransaction = await _deviceContext.Database.BeginTransactionAsync();

            var result = await _userRepository.DeleteUser(userId);

            //LOG THE ACTIONS
            LogWriter.WriteLog(result.Logs);

            if (result.StatusCode == Utils.Success)
            {
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "PostDeleteUser",
                    AuditReportActivityResourceId = new List<int>() { userId }
                });

                if (auditResult.StatusCode != Utils.Success)
                {
                    await dbTransaction.RollbackAsync();

                    return StatusCode(StatusCodes.Status400BadRequest, new ControllerReturnResponse<UserResponse>()
                    {
                        ResponseCode = HttpStatusCode.BadRequest,
                        ResponseDescription = Utils.StatusMessageAuditReportError,
                        StatusCode = Utils.AuditReportError,
                        StatusMessage = Utils.StatusMessageAuditReportError
                    });
                }

                await dbTransaction.CommitAsync();

                return StatusCode(StatusCodes.Status200OK, new ControllerReturnResponse<UserResponse>()
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

                return StatusCode(StatusCodes.Status404NotFound, new ControllerReturnResponse<UserResponse>()
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

                return StatusCode(StatusCodes.Status400BadRequest, new ControllerReturnResponse<UserResponse>()
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