using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Ayuda_Help_Desk.Data;
using Ayuda_Help_Desk.Dtos.AuditReport;
using Ayuda_Help_Desk.Dtos.General;
using Ayuda_Help_Desk.Dtos.RoleFunctionality;
using Ayuda_Help_Desk.Dtos.UserManagement;
using Ayuda_Help_Desk.Helpers;
using Ayuda_Help_Desk.Interfaces;
using Ayuda_Help_Desk.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ayuda_Help_Desk.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class RoleManagementController : ControllerBase
    {
        private readonly IRoleManagementRepository _roleManagementRepository;
        private readonly IMapper _mapper;
        private readonly DataContext _dataContext;
        private readonly IAuditReportRepository _auditReportRepository;

        public RoleManagementController(IRoleManagementRepository roleManagementRepository, IMapper mapper, DataContext dataContext, IAuditReportRepository auditReportRepository)
        {
            _roleManagementRepository = roleManagementRepository;
            _mapper = mapper;
            _dataContext = dataContext;
            _auditReportRepository = auditReportRepository;
        }

        /// <summary>
        /// CREATE ROLE IN THE SYSTEM
        /// </summary>
        [RequiredFunctionalityName("PostRoles")]
        [HttpPost("Roles/Create")]
        public async Task<ActionResult> PostRoles(List<RoleRequest> roles)
        {
            var dbTransaction = await _dataContext.Database.BeginTransactionAsync();
            var result = await _roleManagementRepository.CreateRoles(_mapper.Map<List<Role>>(roles));

            if (result.StatusCode == Utils.Success)
            {
                var listOfRoles = _mapper.Map<List<RoleResponse>>(((List<Role>)result.ObjectValue));
                result.ObjectValue = listOfRoles;
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "PostRoles",
                    AuditReportActivityResourceId = listOfRoles.Select(a => a.Id).ToList()
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

                return StatusCode(StatusCodes.Status200OK, result.ObjectValue);
            }
            else
            {
                await dbTransaction.RollbackAsync();

                return StatusCode(StatusCodes.Status400BadRequest, result);
            }
        }

        /// <summary>
        /// GET ALL ROLES IN THE SYSTEM
        /// </summary>
        [RequiredFunctionalityName("GetRoles")]
        [HttpGet("Roles")]
        public async Task<ActionResult> GetRoles([FromQuery] UserParams userParams)
        {
            var result = await _roleManagementRepository.GetRoles(userParams);

            if (result.StatusCode == Utils.Success)
            {
                var rolesToReturn = new List<RoleResponse>();
                var roles = (PagedList<Role>)result.ObjectValue;
                var roleList = roles.ToList();
                foreach(var role in roleList)
                {
                    int userCountInRole = role.UserRoles.Count;
                    var roleResponse = _mapper.Map<RoleResponse>(role);
                    roleResponse.NumberOfUsersAssignedToRole = userCountInRole;
                    rolesToReturn.Add(roleResponse);
                }

                result.ObjectValue = rolesToReturn;
                Response.AddPagination(roles.CurrentPage, roles.PageSize, roles.TotalCount, roles.TotalPages);
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "GetRoles",
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

                return StatusCode(StatusCodes.Status200OK, result.ObjectValue);
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, result);
            }
        }

        /// <summary>
        /// GET A ROLE IN THE SYSTEM WITH ROLEID = ID
        /// </summary>
        [RequiredFunctionalityName("GetRole")]
        [HttpGet("Roles/{id}")]
        public async Task<ActionResult> GetRoles(int id)
        {
            var result = await _roleManagementRepository.GetRoles(id);

            if (result.StatusCode == Utils.Success)
            {
                var role = (Role)result.ObjectValue;
                int userCountInRole = role.UserRoles.Count;
                var roleResponse = _mapper.Map<RoleResponse>(role);
                roleResponse.NumberOfUsersAssignedToRole = userCountInRole;
                result.ObjectValue = roleResponse;
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "GetRole",
                    AuditReportActivityResourceId = new List<int>() { id }
                });

                if (auditResult.StatusCode != Utils.Success)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, new ReturnResponse()
                    {
                        StatusCode = Utils.AuditReportError,
                        StatusMessage = Utils.StatusMessageAuditReportError
                    });
                }

                return StatusCode(StatusCodes.Status200OK, result.ObjectValue);
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, result);
            }
        }

        /// <summary>
        /// UPDATE ROLES IN THE SYSTEM
        /// </summary>
        [RequiredFunctionalityName("PutRoles")]
        [HttpPost("Roles/Update")]
        public async Task<ActionResult> PutRoles(List<RoleResponse> roleResponses)
        {
            var dbTransaction = await _dataContext.Database.BeginTransactionAsync();
            var result = await _roleManagementRepository.UpdateRoles(roleResponses);

            if (result.StatusCode == Utils.Success)
            {
                result.ObjectValue = _mapper.Map<List<RoleResponse>>(((List<Role>)result.ObjectValue));
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "PutRoles",
                    AuditReportActivityResourceId = roleResponses.Select(a => a.Id).ToList()
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

                return StatusCode(StatusCodes.Status200OK, result.ObjectValue);
            }
            else
            {
                await dbTransaction.RollbackAsync();

                return StatusCode(StatusCodes.Status400BadRequest, result);
            }
        }

        /// <summary>
        /// DELETE ROLES IN THE SYSTEM
        /// </summary>
        [RequiredFunctionalityName("DeleteRoles")]
        [HttpPost("Roles/Delete")]
        public async Task<ActionResult> DeleteRoles(List<RoleResponse> roles)
        {
            var dbTransaction = await _dataContext.Database.BeginTransactionAsync();
            var result = await _roleManagementRepository.DeleteRoles(roles);

            if (result.StatusCode == Utils.Success)
            {
                result.ObjectValue = _mapper.Map<List<RoleResponse>>(((List<Role>)result.ObjectValue));
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "DeleteRoles",
                    AuditReportActivityResourceId = roles.Select(a => a.Id).ToList()
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

                return StatusCode(StatusCodes.Status200OK, result.ObjectValue);
            }
            else
            {
                await dbTransaction.RollbackAsync();

                return StatusCode(StatusCodes.Status400BadRequest, result);
            }
        }

        /// <summary>
        /// ASSIGN ROLES TO USERS IN THE SYSTEM
        /// </summary>
        [RequiredFunctionalityName("PostAssignRolesToUser")]
        [HttpPost("Users/Roles/Assign")]
        public async Task<ActionResult> PostAssignRolesToUser([FromBody] RoleUserAssignmentRequest roleAssignmentRequest)
        {
            var dbTransaction = await _dataContext.Database.BeginTransactionAsync();
            var result = await _roleManagementRepository.AssignRolesToUser(roleAssignmentRequest);

            if (result.StatusCode == Utils.Success)
            {
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "PostAssignRolesToUser",
                    AuditReportActivityResourceId = roleAssignmentRequest.Users.Select(a => a.Id).ToList()
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

                return StatusCode(StatusCodes.Status200OK, result.ObjectValue);
            }
            else
            {
                await dbTransaction.RollbackAsync();

                return StatusCode(StatusCodes.Status400BadRequest, result);
            }
        }

        /// <summary>
        /// GET ALL THE STAFF USER ROLES IN THE SYSTEM
        /// </summary>
        [RequiredFunctionalityName("GetStaffUsersRoles")]
        [HttpGet("Users/Roles")]
        public async Task<ActionResult> GetStaffUsersRoles([FromQuery] UserParams userParams)
        {
            var result = await _roleManagementRepository.GetStaffUsersRoles(userParams);

            if (result.StatusCode == Utils.Success)
            {
                var staffUsersRoles = (PagedList<UserRole>)result.ObjectValue;
                result.ObjectValue = _mapper.Map<List<StaffUserRoleResponse>>(staffUsersRoles.ToList());
                Response.AddPagination(staffUsersRoles.CurrentPage, staffUsersRoles.PageSize, staffUsersRoles.TotalCount, staffUsersRoles.TotalPages);
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "GetStaffUsersRoles",
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

                return StatusCode(StatusCodes.Status200OK, result.ObjectValue);
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, result);
            }
        }

        /// <summary>
        /// CREATE PROJECT MODULES IN THE SYSTEM
        /// </summary>
        [RequiredFunctionalityName("PostProjectModules")]
        [HttpPost("ProjectModules/Create")]
        public async Task<ActionResult> PostProjectModule(List<ProjectModuleRequest> projectModules)
        {
            var dbTransaction = await _dataContext.Database.BeginTransactionAsync();
            var result = await _roleManagementRepository.CreateProjectModule(_mapper.Map<List<ProjectModule>>(projectModules));

            if (result.StatusCode == Utils.Success)
            {
                var listOfProjectModules = _mapper.Map<List<ProjectModuleResponse>>(((List<ProjectModule>)result.ObjectValue));
                result.ObjectValue = listOfProjectModules;
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "PostProjectModules",
                    AuditReportActivityResourceId = listOfProjectModules.Select(a => a.ProjectModuleId).ToList()
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

                return StatusCode(StatusCodes.Status200OK, result.ObjectValue);
            }
            else
            {
                await dbTransaction.RollbackAsync();

                return StatusCode(StatusCodes.Status400BadRequest, result);
            }
        }

        /// <summary>
        /// GET ALL PROJECT MODULES TOGETHER WITH THEIR FUNCTIONAITIES IN THE SYSTEM
        /// </summary>
        [RequiredFunctionalityName("GetProjectModules")]
        [HttpGet("ProjectModules")]
        public async Task<ActionResult> GetProjectModules([FromQuery] UserParams userParams)
        {
            var result = await _roleManagementRepository.GetProjectModules(userParams);

            if (result.StatusCode == Utils.Success)
            {
                var projectModules = (PagedList<ProjectModule>)result.ObjectValue;
                result.ObjectValue = _mapper.Map<List<ProjectModuleResponse>>(projectModules.ToList());
                Response.AddPagination(projectModules.CurrentPage, projectModules.PageSize, projectModules.TotalCount, projectModules.TotalPages);
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "GetProjectModules",
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

                return StatusCode(StatusCodes.Status200OK, result.ObjectValue);
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, result);
            }
        }

        /// <summary>
        /// GET ALL PROJECT MODULES TOGETHER WITH THEIR FUNCTIONAITIES IN THE SYSTEM
        /// </summary>
        [RequiredFunctionalityName("GetProjectModule")]
        [HttpGet("ProjectModules/{projectModuleId}")]
        public async Task<ActionResult> GetProjectModule([FromRoute] int projectModuleId)
        {
            var result = await _roleManagementRepository.GetProjectModules(projectModuleId);

            if (result.StatusCode == Utils.Success)
            {
                result.ObjectValue = _mapper.Map<ProjectModuleResponse>((ProjectModule)result.ObjectValue);
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "GetProjectModule",
                    AuditReportActivityResourceId = new List<int>() { projectModuleId }
                });

                if (auditResult.StatusCode != Utils.Success)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, new ReturnResponse()
                    {
                        StatusCode = Utils.AuditReportError,
                        StatusMessage = Utils.StatusMessageAuditReportError
                    });
                }

                return StatusCode(StatusCodes.Status200OK, result.ObjectValue);
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, result);
            }
        }

        /// <summary>
        /// DELETE PROJECT MODULES IN THE SYSTEM
        /// </summary>
        [RequiredFunctionalityName("DeleteProjectModule")]
        [HttpPost("ProjectModules/Delete")]
        public async Task<ActionResult> DeleteProjectModule(List<ProjectModuleResponse> projectModules)
        {
            var dbTransaction = await _dataContext.Database.BeginTransactionAsync();
            var result = await _roleManagementRepository.DeleteProjectModule(projectModules);

            if (result.StatusCode == Utils.Success)
            {
                result.ObjectValue = _mapper.Map<List<ProjectModuleResponse>>(((List<ProjectModule>)result.ObjectValue));
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "DeleteProjectModule",
                    AuditReportActivityResourceId = projectModules.Select(a => a.ProjectModuleId).ToList()
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

                return StatusCode(StatusCodes.Status200OK, result.ObjectValue);
            }
            else
            {
                await dbTransaction.RollbackAsync();

                return StatusCode(StatusCodes.Status400BadRequest, result);
            }
        }

        /// <summary>
        /// CREATE FUNCTIONALITIES IN THE SYSTEM
        /// </summary>
        //[RequiredFunctionalityName("PostFunctionality")]
        [RequiredFunctionalityName("PostFunctionalities")]
        [HttpPost("Functionalities/Create")]
        public async Task<ActionResult> PostFunctionality(List<FunctionalityRequest> functionalities)
        {
            var dbTransaction = await _dataContext.Database.BeginTransactionAsync();
            var result = await _roleManagementRepository.CreateFunctionality(_mapper.Map<List<Functionality>>(functionalities));

            if (result.StatusCode == Utils.Success)
            {
                var listOfFunctionalities = _mapper.Map<List<FunctionalityResponse>>(((List<Functionality>)result.ObjectValue));
                result.ObjectValue = listOfFunctionalities;
                //AUDIT THIS ACTIVITY FOR THE USER
                //var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                //{
                //    AuditReportActivityFunctionalityName = "PostFunctionalities",
                //    AuditReportActivityResourceId = listOfFunctionalities.Select(a => a.FunctionalityId).ToList()
                //});

                //if (auditResult.StatusCode != Utils.Success)
                //{
                //    await dbTransaction.RollbackAsync();

                //    return StatusCode(StatusCodes.Status400BadRequest, new ReturnResponse()
                //    {
                //        StatusCode = Utils.AuditReportError,
                //        StatusMessage = Utils.StatusMessageAuditReportError
                //    });
                //}

                await dbTransaction.CommitAsync();

                return StatusCode(StatusCodes.Status200OK, result.ObjectValue);
            }
            else
            {
                await dbTransaction.RollbackAsync();

                return StatusCode(StatusCodes.Status400BadRequest, result);
            }
        }

        /// <summary>
        /// GET FUNCTIONALITIES IN THE SYSTEM (USE GET PROJECT MODULES ROUTE)
        /// </summary>
        [RequiredFunctionalityName("GetFunctionalities")]
        [HttpGet("Functionalities")]
        public async Task<ActionResult> GetFunctionalities([FromQuery] UserParams userParams)
        {
            var result = await _roleManagementRepository.GetFunctionalities(userParams);

            if (result.StatusCode == Utils.Success)
            {
                var functionalities = (PagedList<Functionality>)result.ObjectValue;
                result.ObjectValue = _mapper.Map<List<FunctionalityResponse>>(functionalities.ToList());
                Response.AddPagination(functionalities.CurrentPage, functionalities.PageSize, functionalities.TotalCount, functionalities.TotalPages);
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "GetFunctionalities",
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

                return StatusCode(StatusCodes.Status200OK, result.ObjectValue);
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, result);
            }
        }

        /// <summary>
        /// GET FUNCTIONALITIES IN THE SYSTEM (USE GET PROJECT MODULES ROUTE)
        /// </summary>
        [RequiredFunctionalityName("GetFunctionality")]
        [HttpGet("Functionalities/{functionalityId}")]
        public async Task<ActionResult> GetFunctionalities([FromRoute] int functionalityId)
        {
            var result = await _roleManagementRepository.GetFunctionalities(functionalityId);

            if (result.StatusCode == Utils.Success)
            {
                result.ObjectValue = _mapper.Map<FunctionalityResponse>((Functionality)result.ObjectValue);
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "GetFunctionality",
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

                return StatusCode(StatusCodes.Status200OK, result.ObjectValue);
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, result);
            }
        }

        /// <summary>
        /// DELETE FUNCTIONALITIES IN THE SYSTEM
        /// </summary>
        [RequiredFunctionalityName("DeleteFunctionality")]
        [HttpPost("Functionalities/Delete")]
        public async Task<ActionResult> DeleteFunctionality(List<FunctionalityResponse> functionalities)
        {
            var dbTransaction = await _dataContext.Database.BeginTransactionAsync();
            var result = await _roleManagementRepository.DeleteFunctionality(functionalities);

            if (result.StatusCode == Utils.Success)
            {
                result.ObjectValue = _mapper.Map<List<FunctionalityResponse>>(((List<Functionality>)result.ObjectValue));
                //AUDIT THIS ACTIVITY FOR THE USER
                var auditResult = await _auditReportRepository.CreateAuditReport(new AuditReportRequest()
                {
                    AuditReportActivityFunctionalityName = "DeleteFunctionality",
                    AuditReportActivityResourceId = functionalities.Select(a => a.FunctionalityId).ToList()
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

                return StatusCode(StatusCodes.Status200OK, result.ObjectValue);
            }
            else
            {
                await dbTransaction.RollbackAsync();

                return StatusCode(StatusCodes.Status400BadRequest, result);
            }
        }
    }
}
