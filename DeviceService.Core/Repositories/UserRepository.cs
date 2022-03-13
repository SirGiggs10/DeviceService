using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DeviceService.Core.Dtos.Global;
using DeviceService.Core.Dtos.User;
using AutoMapper;
using DeviceService.Core.Interfaces.Repositories;
using DeviceService.Core.Data.DataContext;
using DeviceService.Core.Helpers.Pagination;
using DeviceService.Core.Helpers.Extensions;
using DeviceService.Core.Helpers.Common;
using DeviceService.Core.Helpers.Logging.Logger;
using static DeviceService.Core.Helpers.Common.Utils;
using DeviceService.Core.Entities;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using DeviceService.Core.Dtos.RoleFunctionality;
using DeviceService.Core.Dtos.Auth;
using System.Security.Claims;

namespace DeviceService.Core.Repositories
{
    public class UserRepository : IUserRepository
    {
        private string className = string.Empty;

        private readonly IMapper _mapper;
        private readonly IGlobalRepository _globalRepository;
        private readonly DeviceContext _deviceContext;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly IRoleManagementRepository _roleManagementRepository;

        public UserRepository(IMapper mapper, IGlobalRepository globalRepository, DeviceContext deviceContext, UserManager<User> userManager, RoleManager<Role> roleManager, IRoleManagementRepository roleManagementRepository)
        {
            className = GetType().Name;

            _mapper = mapper;
            _globalRepository = globalRepository;
            _deviceContext = deviceContext;
            _userManager = userManager;
            _roleManager = roleManager;
            _roleManagementRepository = roleManagementRepository;
        }

        public async Task<ReturnResponse<UserResponse>> CreateUser(UserRequest userRequest)
        {
            string methodName = "CreateUser"/*MethodBase.GetCurrentMethod().Name*/, classAndMethodName = $"{className}.{methodName}";

            var logs = new List<Log>();
            var logBuilder = new StringBuilder($"--------------{classAndMethodName}--------START--------").AppendLine();
            logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Received Request for Creating User. Payload: {JsonConvert.SerializeObject(userRequest)}").AppendLine();

            try
            {
                //GET USER ROLE
                var role = await _roleManager.FindByNameAsync(userRequest.RoleToAssignToUser);
                if (role == null)
                {
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Role Not Found").AppendLine();
                    logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                    logBuilder.ToString().AddToLogs(ref logs);

                    return new ReturnResponse<UserResponse>()
                    {
                        StatusCode = Utils.NotFound,
                        StatusMessage = "Role is Not Found",
                        Logs = logs
                    };
                }

                //MAKE SURE ONLY ADMIN REGISTERS ADMINS
                if(role.Name == Utils.Role_Administrator)
                {
                    //AUTHENTICATE LOGGED IN USER
                    var userClaim = MyHttpContextAccessor.GetHttpContextAccessor().HttpContext?.User?.Claims?.FirstOrDefault(a => a.Type == ClaimTypes.Role);

                    if(!((userClaim != null) && (userClaim.Value == Utils.Role_Administrator)))
                    {
                        logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Administrator User Claim Not Found or Logged In User does not have Administrator Role").AppendLine();
                        logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                        logBuilder.ToString().AddToLogs(ref logs);

                        return new ReturnResponse<UserResponse>()
                        {
                            StatusCode = Utils.BadRequest,
                            StatusMessage = "User Not Allowed to Create Administrator Users",
                            Logs = logs
                        };
                    }
                }

                var userRoleForAssignment = new RoleResponse()
                {
                    Id = role.Id
                };

                if (await UserExists(userRequest.EmailAddress))
                {
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} User Role Not Found").AppendLine();
                    logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                    logBuilder.ToString().AddToLogs(ref logs);

                    return new ReturnResponse<UserResponse>()
                    {
                        StatusCode = Utils.ObjectExists,
                        StatusMessage = "User Already Exists",
                        Logs = logs
                    };
                }

                var user = new User
                {
                    Email = userRequest.EmailAddress,
                    UserName = userRequest.EmailAddress,
                    FullName = userRequest.FullName,
                    PhoneNumber = userRequest.PhoneNumber,
                    Address = userRequest.Address,
                    UserType = Utils.UserType_User,
                    Deleted = false
                };

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} About to Create User on the Database").AppendLine();
                
                var result = _userManager.CreateAsync(user, userRequest.Password).Result;
                
                if (result.Succeeded)
                {
                    //var assignmentResult = await _userManager.AddToRolesAsync(user, rolesToAssign.AsEnumerable());
                    var assignmentResult = await _roleManagementRepository.AssignRolesToUser(new RoleUserAssignmentRequest()
                    {
                        Users = new List<UserToReturn>() {
                                new UserToReturn()
                                {
                                    Id = user.Id
                                }
                            },
                        Roles = new List<RoleResponse>()
                            {
                                userRoleForAssignment
                            }
                    });

                    if (assignmentResult.StatusCode == Utils.Success)
                    {
                        logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} User Creation was Successful").AppendLine();
                        logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                        logBuilder.ToString().AddToLogs(ref logs);

                        return new ReturnResponse<UserResponse>()
                        {
                            StatusCode = Utils.StatusCode_Success,
                            StatusMessage = Utils.StatusMessage_Success,
                            ObjectValue = _mapper.Map<UserResponse>(user),
                            Logs = logs
                        };
                    }

                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Assigning Role to User was not Successful").AppendLine();
                    logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                    logBuilder.ToString().AddToLogs(ref logs);

                    return new ReturnResponse<UserResponse>()
                    {
                        StatusCode = Utils.NotSucceeded,
                        StatusMessage = "Unable to Create User...Try Again Later",
                        Logs = logs
                    };
                }

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Creating User was not Successful").AppendLine();
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                return new ReturnResponse<UserResponse>()
                {
                    StatusCode = Utils.NotSucceeded,
                    StatusMessage = "Unable to Create User...Try Again Later",
                    Logs = logs
                };
            }
            catch (Exception ex)
            {
                //ON EXCEPTION STORE THE PREVIOUS LOG
                LogWriter.AddLogAndClearLogBuilderOnException(ref logBuilder, LogType.LOG_DEBUG, ref logs, ex, "CreateUser Exception");

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Error Encountered while Inserting User to the Database").AppendLine();
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                return new ReturnResponse<UserResponse>()
                {
                    StatusCode = Utils.StatusCode_ExceptionError,
                    StatusMessage = "Unable to Create User...Try Again Later",
                    Logs = logs
                };
            }
        }

        private async Task<bool> UserExists(string emailAddress)
        {
            return await _userManager.Users.AnyAsync(a => a.NormalizedEmail == emailAddress.ToUpper());
        }

        public async Task<ReturnResponse<UserResponse>> DeleteUser(int userId)
        {
            string methodName = "DeleteUser"/*MethodBase.GetCurrentMethod().Name*/, classAndMethodName = $"{className}.{methodName}";

            var logs = new List<Log>();
            var logBuilder = new StringBuilder($"--------------{classAndMethodName}--------START--------").AppendLine();
            logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Received Request for Deleting Users from The Database. Payload: {userId}").AppendLine();

            try
            {
                var user = await _userManager.Users.Where(a => a.Id == userId).Include(b => b.UserRoles).FirstOrDefaultAsync();

                if (user == null)
                {
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} User Not Found. Deleting Record from the Database was not Successful. RecordId: {userId}").AppendLine();
                    logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                    logBuilder.ToString().AddToLogs(ref logs);

                    return new ReturnResponse<UserResponse>()
                    {
                        StatusCode = Utils.NotFound,
                        StatusMessage = "User Not Found. Unable to Delete User...Try Again Later",
                        Logs = logs
                    };
                }

                //DELETE RECORDS FROM DEVICE CONTEXT. START TRACKING
                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} About to Delete User from the Database").AppendLine();
                var deletionResult = await _userManager.DeleteAsync(user);

                if (!deletionResult.Succeeded)
                {
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Deleting Record from the Database was not Successful. Errors: {JsonConvert.SerializeObject(deletionResult.Errors)}").AppendLine();
                    logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                    logBuilder.ToString().AddToLogs(ref logs);

                    return new ReturnResponse<UserResponse>()
                    {
                        StatusCode = Utils.NotSucceeded,
                        StatusMessage = "Unable to Delete Users...Try Again Later",
                        Logs = logs
                    };
                }

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Deleting Records from the DB was Successful").AppendLine();
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                return new ReturnResponse<UserResponse>()
                {
                    StatusCode = Utils.StatusCode_Success,
                    StatusMessage = Utils.StatusMessage_Success,
                    ObjectValue = _mapper.Map<UserResponse>(user),
                    Logs = logs
                };
            }
            catch (Exception ex)
            {
                //ON EXCEPTION STORE THE PREVIOUS LOG
                LogWriter.AddLogAndClearLogBuilderOnException(ref logBuilder, LogType.LOG_DEBUG, ref logs, ex, "DeleteUser Exception");

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Error Encountered while Deleting User from the Database").AppendLine();
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                return new ReturnResponse<UserResponse>()
                {
                    StatusCode = Utils.StatusCode_ExceptionError,
                    StatusMessage = "Unable to Delete User...Try Again Later",
                    Logs = logs
                };
            }
        }

        public async Task<ReturnResponse<UserResponse>> GetUser(int userId)
        {
            string methodName = "GetUser"/*MethodBase.GetCurrentMethod().Name*/, classAndMethodName = $"{className}.{methodName}";

            var logs = new List<Log>();
            var logBuilder = new StringBuilder($"--------------{classAndMethodName}--------START--------").AppendLine();
            logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Received Request for Getting User from The Database. Payload: {userId}").AppendLine();

            try
            {
                var user = await _userManager.Users.Where(a => a.Id == userId).Include(b => b.UserRoles).ThenInclude(c => c.Role).FirstOrDefaultAsync();

                if (user == null)
                {
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} User Not Found.").AppendLine();
                    logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                    logBuilder.ToString().AddToLogs(ref logs);

                    return new ReturnResponse<UserResponse>()
                    {
                        StatusCode = Utils.NotFound,
                        StatusMessage = "User Not Found.",
                        Logs = logs
                    };
                }

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Fetching Record from the DB was Successful").AppendLine();
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                return new ReturnResponse<UserResponse>()
                {
                    StatusCode = Utils.StatusCode_Success,
                    StatusMessage = Utils.StatusMessage_Success,
                    ObjectValue = _mapper.Map<UserResponse>(user),
                    Logs = logs
                };
            }
            catch (Exception ex)
            {
                //ON EXCEPTION STORE THE PREVIOUS LOG
                LogWriter.AddLogAndClearLogBuilderOnException(ref logBuilder, LogType.LOG_DEBUG, ref logs, ex, "GetUser Exception");

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Error Encountered while Getting User from the Database").AppendLine();
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                return new ReturnResponse<UserResponse>()
                {
                    StatusCode = Utils.StatusCode_ExceptionError,
                    StatusMessage = "Unable to Get User...Try Again Later",
                    Logs = logs
                };
            }
        }

        public async Task<ReturnResponse<List<UserResponse>>> GetUsers(UserParams userParam)
        {
            string methodName = "GetUsers"/*MethodBase.GetCurrentMethod().Name*/, classAndMethodName = $"{className}.{methodName}";

            var logs = new List<Log>();
            var logBuilder = new StringBuilder($"--------------{classAndMethodName}--------START--------").AppendLine();
            logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Received Request for Getting Users from The Database.").AppendLine();

            try
            {
                var users = _userManager.Users.Include(a => a.UserRoles).ThenInclude(b => b.Role);

                var pagedList = await PagedList<User>.CreateAsync(users, userParam.PageNumber, userParam.PageSize);
                MyHttpContextAccessor.GetHttpContextAccessor().HttpContext.Response.AddPagination(pagedList.CurrentPage, pagedList.PageSize, pagedList.TotalCount, pagedList.TotalPages);
                var listOfUsersToReturn = pagedList.ToList();

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Fetching Records from the DB was Successful").AppendLine();
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                return new ReturnResponse<List<UserResponse>>()
                {
                    StatusCode = Utils.StatusCode_Success,
                    StatusMessage = Utils.StatusMessage_Success,
                    ObjectValue = _mapper.Map<List<UserResponse>>(listOfUsersToReturn),
                    Logs = logs
                };
            }
            catch (Exception ex)
            {
                //ON EXCEPTION STORE THE PREVIOUS LOG
                LogWriter.AddLogAndClearLogBuilderOnException(ref logBuilder, LogType.LOG_DEBUG, ref logs, ex, "GetUsers Exception");

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Error Encountered while Getting Users from the Database").AppendLine();
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                return new ReturnResponse<List<UserResponse>>()
                {
                    StatusCode = Utils.StatusCode_ExceptionError,
                    StatusMessage = "Unable to Get Users...Try Again Later",
                    Logs = logs
                };
            }
        }

        public async Task<ReturnResponse<UserResponse>> UpdateUser(int userId, UserToUpdate userToUpdate)
        {
            string methodName = "UpdateUser"/*MethodBase.GetCurrentMethod().Name*/, classAndMethodName = $"{className}.{methodName}";

            var logs = new List<Log>();
            var logBuilder = new StringBuilder($"--------------{classAndMethodName}--------START--------").AppendLine();
            logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Received Request for Updating User to The Database. Payload: {JsonConvert.SerializeObject(userToUpdate)}").AppendLine();

            if (userId != userToUpdate.Id)
            {
                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Users Ids did not Match.").AppendLine();
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                return new ReturnResponse<UserResponse>()
                {
                    StatusCode = Utils.BadRequest,
                    StatusMessage = "Unable to Update User...Try Again Later",
                    Logs = logs
                };
            }

            try
            {
                var user = await _userManager.Users.FirstOrDefaultAsync(a => a.Id == userId);
                if (user == null)
                {
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} User Not Found.").AppendLine();
                    logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                    logBuilder.ToString().AddToLogs(ref logs);

                    return new ReturnResponse<UserResponse>()
                    {
                        StatusCode = Utils.NotFound,
                        StatusMessage = "User Not Found. Unable to Update User...Try Again Later",
                        Logs = logs
                    };
                }

                var userToUpdateMain = _mapper.Map(userToUpdate, user);
                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} About to Update User to the Database").AppendLine();
                //UPDATE RECORD TO DEVICE CONTEXT. START TRACKING
                var result = await _userManager.UpdateAsync(userToUpdateMain);

                if (!result.Succeeded)
                {
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Updating to the Database was not Successful. Errors: {JsonConvert.SerializeObject(result.Errors)}").AppendLine();
                    logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                    logBuilder.ToString().AddToLogs(ref logs);

                    return new ReturnResponse<UserResponse>()
                    {
                        StatusCode = Utils.NotSucceeded,
                        StatusMessage = "Unable to Update User...Try Again Later",
                        Logs = logs
                    };
                }

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Updating Record to the DB was Successful").AppendLine();
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                return new ReturnResponse<UserResponse>()
                {
                    StatusCode = Utils.StatusCode_Success,
                    StatusMessage = Utils.StatusMessage_Success,
                    ObjectValue = _mapper.Map<UserResponse>(user),
                    Logs = logs
                };
            }
            catch (Exception ex)
            {
                //ON EXCEPTION STORE THE PREVIOUS LOG
                LogWriter.AddLogAndClearLogBuilderOnException(ref logBuilder, LogType.LOG_DEBUG, ref logs, ex, "UpdateUser Exception");

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Error Encountered while Updating User to the Database").AppendLine();
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                return new ReturnResponse<UserResponse>()
                {
                    StatusCode = Utils.StatusCode_ExceptionError,
                    StatusMessage = "Unable to Update User...Try Again Later",
                    Logs = logs
                };
            }
        }
    }
}
