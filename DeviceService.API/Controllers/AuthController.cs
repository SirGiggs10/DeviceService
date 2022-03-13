using System;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using DeviceService.Core.Helpers.RoleBasedAccess;
using DeviceService.Core.Dtos.Auth;
using DeviceService.Core.Helpers.Common;
using DeviceService.Core.Entities;
using DeviceService.Core.Interfaces.Repositories;
using DeviceService.Core.Data.DataContext;
using DeviceService.Core.Dtos.AuditReport;
using DeviceService.Core.Dtos.Global;
using System.Net;
using DeviceService.Core.Dtos.User;

namespace Ayuda_Help_Desk.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly IAuthRepository _authRepository;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IAuditReportRepository _auditReportRepository;
        private readonly DeviceContext _dataContext;

        public AuthController(UserManager<User> userManager, IAuthRepository authRepository, IMapper mapper, IConfiguration configuration, IAuditReportRepository auditReportRepository, DeviceContext dataContext)
        {
            _userManager = userManager;
            _authRepository = authRepository;
            _mapper = mapper;
            _configuration = configuration;
            _auditReportRepository = auditReportRepository;
            _dataContext = dataContext;
        }

        // POST api/Auth/Login
        /// <summary>
        /// LOGIN USER TO THE SYSTEM
        /// </summary>
        /// <param name="userForLoginDto">User Login Request Body</param>
        /// <returns>User Login Details with Token</returns>
        /// <response code="200">Returns User Login Details with Token</response>
        /// <response code="400">If Login Request is Bad</response>
        /// <response code="500">If Unknown Error Occurs</response>
        [ProducesResponseType(typeof(ControllerReturnResponse<UserLoginResponseForLogin>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status500InternalServerError)]
        [Consumes("application/json")]
        [AllowAnonymous]
        [HttpPost]
        [Route("Login")]
        public async Task<ActionResult<ControllerReturnResponse<UserLoginResponseForLogin>>> PostLogin([FromBody] UserForLoginDto userForLoginDto)
        {
            var dbTransaction = await _dataContext.Database.BeginTransactionAsync();

            var result = await _authRepository.LoginUser(userForLoginDto);

            if (result.StatusCode == Utils.Success)
            {
                var userDetails = (UserDetails)result.ObjectValue;
                var userInfoToReturn = _mapper.Map<UserLoginResponseForLogin>(userDetails);

                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "PostLogin",
                    AuditReportActivityResourceId = new List<int>() { },
                    UserId = userInfoToReturn.User.Id
                });

                if (auditResult.StatusCode != Utils.Success)
                {
                    await dbTransaction.RollbackAsync();

                    return StatusCode(StatusCodes.Status400BadRequest, new ControllerReturnResponse<UserLoginResponseForLogin>()
                    {
                        ResponseCode = HttpStatusCode.BadRequest,
                        ResponseDescription = Utils.StatusMessageAuditReportError,
                        StatusCode = Utils.AuditReportError,
                        StatusMessage = Utils.StatusMessageAuditReportError
                    });
                }

                await dbTransaction.CommitAsync();

                return StatusCode(StatusCodes.Status400BadRequest, new ControllerReturnResponse<UserLoginResponseForLogin>()
                {
                    ResponseCode = HttpStatusCode.OK,
                    ResponseDescription = result.StatusMessage,
                    StatusCode = result.StatusCode,
                    StatusMessage = result.StatusMessage,
                    ObjectValue = userInfoToReturn
                });
            }
            else
            {
                await dbTransaction.RollbackAsync();

                return StatusCode(StatusCodes.Status200OK, new ControllerReturnResponse<UserLoginResponseForLogin>()
                {
                    ResponseCode = HttpStatusCode.BadRequest,
                    ResponseDescription = result.StatusMessage,
                    StatusCode = result.StatusCode,
                    StatusMessage = result.StatusMessage
                });
            }
        }

        // POST api/Auth/ChangePassword
        /// <summary>
        /// CHANGE USER PASSWORD
        /// </summary>
        /// <param name="changePasswordRequest">Change Password Request Body</param>
        /// <returns>User Details</returns>
        /// <response code="200">Returns User Details</response>
        /// <response code="400">If ChangePassword Request is Bad</response>
        /// <response code="500">If Unknown Error Occurs</response>
        [ProducesResponseType(typeof(ControllerReturnResponse<UserLoginResponseForLogin>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status500InternalServerError)]
        [Consumes("application/json")]
        [RequiredFunctionalityName("PostChangePassword")]
        [HttpPost]
        [Route("ChangePassword")]
        public async Task<ActionResult<ControllerReturnResponse<UserResponse>>> PostChangePassword([FromBody] ChangePasswordRequest changePasswordRequest)
        {
            var dbTransaction = await _dataContext.Database.BeginTransactionAsync();

            var result = await _authRepository.ChangePassword(changePasswordRequest);

            if (result.StatusCode == Utils.Success)
            {
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "PostChangePassword",
                    AuditReportActivityResourceId = new List<int>() { }
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