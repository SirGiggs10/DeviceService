using System;
using System.Text;
using System.Threading.Tasks;
using Ayuda_Help_Desk.Models;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Ayuda_Help_Desk.Dtos;
using Ayuda_Help_Desk.Interfaces;
using Microsoft.AspNetCore.Http;
using Ayuda_Help_Desk.Helpers;
using System.Net;
using Ayuda_Help_Desk.Dtos.Customer;
using Ayuda_Help_Desk.Dtos.General;
using Microsoft.AspNetCore.Authorization;
using Ayuda_Help_Desk.DTOs.Auth;
using Ayuda_Help_Desk.Dtos.Auth;
using Ayuda_Help_Desk.Dtos.Staff;
using Ayuda_Help_Desk.Dtos.UserManagement;
using Ayuda_Help_Desk.Data;
using Ayuda_Help_Desk.Dtos.AuditReport;
using System.Collections.Generic;

namespace Ayuda_Help_Desk.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly ICustomerRepository _customerRepository;
        private readonly IAuthRepository _authRepository;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IMailController _mailController;
        private readonly IAuditReportRepository _auditReportRepository;
        private readonly DataContext _dataContext;

        public AuthController(UserManager<User> userManager, IAuthRepository authRepository, ICustomerRepository customerRepository, IMapper mapper, IConfiguration configuration, IMailController mailController, IAuditReportRepository auditReportRepository, DataContext dataContext)
        {
            _userManager = userManager;
            _authRepository = authRepository;
            _customerRepository = customerRepository;
            _mapper = mapper;
            _configuration = configuration;
            _mailController = mailController;
            _auditReportRepository = auditReportRepository;
            _dataContext = dataContext;
        }

        /// <summary>
        /// LOGIN THE USER TO THE SYSTEM
        /// </summary>
        [AllowAnonymous]
        [HttpPost]
        [Route("Login")]
        public async Task<ActionResult> PostLogin([FromBody] UserForLoginDto userForLoginDto)
        {
            var dbTransaction = await _dataContext.Database.BeginTransactionAsync();
            var result = await _authRepository.LoginUser(userForLoginDto, _configuration.GetValue<string>("AppSettings:Secret"));

            if (result.StatusCode == Utils.Success)
            {
                result.StatusMessage = "Login Success!!!";
                var userDetails = (UserDetails)result.ObjectValue;
                var userInfoToReturn = _mapper.Map<UserLoginResponseForLogin>(userDetails);
                if (userDetails.User.UserType == Utils.Customer)
                {
                    //CUSTOMER
                    userInfoToReturn.UserProfileInformation = _mapper.Map<CustomerResponse>((Customer)userDetails.userProfile);
                }
                else
                {
                    //STAFF
                    userInfoToReturn.UserProfileInformation = _mapper.Map<StaffResponse>((Staff)userDetails.userProfile);
                }

                result.ObjectValue = userInfoToReturn;
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

        /// <summary>
        /// VERIFY MOBILE USER EMAIL ADDRESS
        /// </summary>
        [AllowAnonymous]
        [HttpPost("VerifyMobileUserEmail")]
        public async Task<ActionResult> PostVerifyUserMobileEmailAddress([FromBody] UserEmailCode otp)
        {
            var dbTransaction = await _dataContext.Database.BeginTransactionAsync();
            var result = await _authRepository.VerifyUserMobileEmailAddress(otp);

            if (result.StatusCode == Utils.Success)
            {
                var userDetails = (UserDetails)result.ObjectValue;
                var userInfoToReturn = _mapper.Map<UserLoginResponse>(userDetails);
                if (userDetails.User.UserType == Utils.Customer)
                {
                    //CUSTOMER
                    userInfoToReturn.UserProfileInformation = _mapper.Map<CustomerResponse>((Customer)userDetails.userProfile);
                }
                else
                {
                    //STAFF
                    userInfoToReturn.UserProfileInformation = _mapper.Map<StaffResponse>((Staff)userDetails.userProfile);
                }

                result.ObjectValue = userInfoToReturn;
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "PostVerifyUserEmailAddress",
                    AuditReportActivityResourceId = new List<int>() { },
                    UserId = userInfoToReturn.User.Id
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

        /// <summary>
        /// VERIFY USER EMAIL ADDRESS
        /// </summary>
        [AllowAnonymous]
        [HttpPost("VerifyUserEmail")]
        public async Task<ActionResult> PostVerifyUserEmailAddress([FromBody] UserEmailRequest userEmailRequest)
        {
            var dbTransaction = await _dataContext.Database.BeginTransactionAsync();
            var result = await _authRepository.VerifyUserEmailAddress(userEmailRequest, _configuration.GetValue<string>("AppSettings:Secret"));

            if (result.StatusCode == Utils.Success)
            {
                var userDetails = (UserDetails)result.ObjectValue;
                var userInfoToReturn = _mapper.Map<UserLoginResponse>(userDetails);
                if (userDetails.User.UserType == Utils.Customer)
                {
                    //CUSTOMER
                    userInfoToReturn.UserProfileInformation = _mapper.Map<CustomerResponse>((Customer)userDetails.userProfile);
                }
                else
                {
                    //STAFF
                    userInfoToReturn.UserProfileInformation = _mapper.Map<StaffResponse>((Staff)userDetails.userProfile);
                }

                result.ObjectValue = userInfoToReturn;
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "PostVerifyUserEmailAddress",
                    AuditReportActivityResourceId = new List<int>() { },
                    UserId = userInfoToReturn.User.Id
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

        /// <summary>
        /// SEND PASSWORD RESET LINK TO THE USER'S EMAIL
        /// </summary>
        [RequiredFunctionalityName("PostChangePassword")]
        [HttpPost]
        [Route("ChangePassword")]
        public async Task<ActionResult> PostChangePassword([FromForm] ChangePasswordRequest changePasswordRequest)
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

        /// <summary>
        /// SEND PASSWORD RESET LINK TO THE USER'S EMAIL
        /// </summary>
        [AllowAnonymous]
        [HttpPost]
        [Route("ResetPassword/SendMail")]
        public async Task<ActionResult> PostResetPasswordSendMail([FromBody] ResetPasswordSendMailRequest resetPasswordRequest)
        {
            var dbTransaction = await _dataContext.Database.BeginTransactionAsync();
            var result = await _authRepository.ResetPasswordSendMail(resetPasswordRequest);

            if (result.StatusCode == Utils.Success)
            {
                var userInfoToReturn = _mapper.Map<UserToReturn>((User)result.ObjectValue);
                result.ObjectValue = userInfoToReturn;
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "PostResetPasswordSendMail",
                    AuditReportActivityResourceId = new List<int>() { },
                    UserId = userInfoToReturn.Id
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

        /// <summary>
        /// SEND PASSWORD RESET CODE TO THE USER'S EMAIL
        /// </summary>
        [AllowAnonymous]
        [HttpPost]
        [Route("ResetPassword/SendMailCode")]
        public async Task<ActionResult> PostResetPasswordSendMailCode([FromBody] ResetPasswordSendMailRequest resetPasswordRequest)
        {
            var dbTransaction = await _dataContext.Database.BeginTransactionAsync();
            var result = await _authRepository.ResetPasswordSendMailCode(resetPasswordRequest);

            if (result.StatusCode == Utils.Success)
            {
                var userInfoToReturn = _mapper.Map<UserToReturn>((User)result.ObjectValue);
                result.ObjectValue = userInfoToReturn;
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "PostResetPasswordSendMail",
                    AuditReportActivityResourceId = new List<int>() { },
                    UserId = userInfoToReturn.Id
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

        /// <summary>
        /// FINALLY RESET USER PASSWORD USING THE PASSWORD RESET LINK SENT EARLIER AND THEN SET NEW PASSWORD FOR THE USER
        /// </summary>
        [AllowAnonymous]
        [HttpPost]
        [Route("ResetPassword/SetNewPassword")]
        public async Task<ActionResult> PostResetPasswordSetNewPassword([FromBody] ResetPasswordRequest resetPasswordRequest)
        {
            var dbTransaction = await _dataContext.Database.BeginTransactionAsync();
            var result = await _authRepository.ResetPasswordSetNewPassword(resetPasswordRequest);

            if (result.StatusCode == Utils.Success)
            {
                result.StatusMessage = "Login Success!!!";
                var userDetails = (UserDetails)result.ObjectValue;
                var userInfoToReturn = _mapper.Map<UserLoginResponse>(userDetails);
                if (userDetails.User.UserType == Utils.Customer)
                {
                    //CUSTOMER
                    userInfoToReturn.UserProfileInformation = _mapper.Map<CustomerResponse>((Customer)userDetails.userProfile);
                }
                else
                {
                    //STAFF
                    userInfoToReturn.UserProfileInformation = _mapper.Map<StaffResponse>((Staff)userDetails.userProfile);
                }

                result.ObjectValue = userInfoToReturn;
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "PostResetPasswordSetNewPassword",
                    AuditReportActivityResourceId = new List<int>() { },
                    UserId = userInfoToReturn.User.Id
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

        /// <summary>
        /// FINALLY RESET USER PASSWORD USING THE PASSWORD RESET CODE SENT EARLIER AND THEN SET NEW PASSWORD FOR THE USER
        /// </summary>
        [AllowAnonymous]
        [HttpPost]
        [Route("ResetPassword/SetNewPassword/CodeVerification")]
        public async Task<ActionResult> ResetPasswordCodeVerification([FromBody] ResetPasswordCodeRequest resetPasswordRequest)
        {
            var dbTransaction = await _dataContext.Database.BeginTransactionAsync();
            var result = await _authRepository.ResetPasswordCodeVerification(resetPasswordRequest);

            if (result.StatusCode == Utils.Success)
            {
                result.StatusMessage = "Login Success!!!";
                var userDetails = (UserDetails)result.ObjectValue;
                var userInfoToReturn = _mapper.Map<UserLoginResponse>(userDetails);
                if (userDetails.User.UserType == Utils.Customer)
                {
                    //CUSTOMER
                    userInfoToReturn.UserProfileInformation = _mapper.Map<CustomerResponse>((Customer)userDetails.userProfile);
                }
                else
                {
                    //STAFF
                    userInfoToReturn.UserProfileInformation = _mapper.Map<StaffResponse>((Staff)userDetails.userProfile);
                }

                result.ObjectValue = userInfoToReturn;
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "PostResetPasswordSetNewPassword",
                    AuditReportActivityResourceId = new List<int>() { },
                    UserId = userInfoToReturn.User.Id
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

        /// <summary>
        /// RESEND EMAIL VERIFICATION LINK TO USERS EMAIL INCASE HE MISSED THE LINK SENT DURING REGISTRATION
        /// </summary>
        [AllowAnonymous]
        [HttpPost]
        [Route("ResendUserMobileEmailVerificationCode")]
        public async Task<ActionResult> PostResendUserMobileEmailVerificationCode([FromBody] EmailVerificationRequest emailVerificationRequest)
        {
            var dbTransaction = await _dataContext.Database.BeginTransactionAsync();
            var result = await _authRepository.ResendUserMobileEmailVerificationCode(emailVerificationRequest);

            if (result.StatusCode == Utils.Success)
            {
                var userInfoToReturn = _mapper.Map<UserToReturn>((User)result.ObjectValue);
                result.ObjectValue = userInfoToReturn;
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "PostResendUserEmailVerificationLink",
                    AuditReportActivityResourceId = new List<int>() { },
                    UserId = userInfoToReturn.Id
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

        /// <summary>
        /// RESEND EMAIL VERIFICATION LINK TO USERS EMAIL INCASE HE MISSED THE LINK SENT DURING REGISTRATION
        /// </summary>
        [AllowAnonymous]
        [HttpPost]
        [Route("ResendUserEmailVerificationLink")]
        public async Task<ActionResult> PostResendUserEmailVerificationLink([FromBody] EmailVerificationRequest emailVerificationRequest)
        {
            var dbTransaction = await _dataContext.Database.BeginTransactionAsync();
            var result = await _authRepository.ResendUserEmailVerificationLink(emailVerificationRequest);

            if (result.StatusCode == Utils.Success)
            {
                var userInfoToReturn = _mapper.Map<UserToReturn>((User)result.ObjectValue);
                result.ObjectValue = userInfoToReturn;
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "PostResendUserEmailVerificationLink",
                    AuditReportActivityResourceId = new List<int>() { },
                    UserId = userInfoToReturn.Id
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

        /// <summary>
        /// RESEND EMAIL VERIFICATION LINK TO USERS EMAIL INCASE HE MISSED THE LINK SENT DURING REGISTRATION
        /// </summary>
        [RequiredFunctionalityName("GetUserInSystem")]
        [HttpGet]
        [Route("Users/{userId}")]
        public async Task<ActionResult> GetUser([FromRoute] int userId)
        {
            var dbTransaction = await _dataContext.Database.BeginTransactionAsync();
            var result = await _authRepository.GetUser(userId);

            if (result.StatusCode == Utils.Success)
            {
                var userInfoToReturn = _mapper.Map<UserWithUserTypeObjectResponse>((User)result.ObjectValue);
                result.ObjectValue = userInfoToReturn;
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "GetUserInSystem",
                    AuditReportActivityResourceId = new List<int>() { userId }
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