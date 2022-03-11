using AutoMapper;
using DeviceService.Core.Data.DataContext;
using DeviceService.Core.Dtos.Auth;
using DeviceService.Core.Dtos.Global;
using DeviceService.Core.Dtos.RoleFunctionality;
using DeviceService.Core.Entities;
using DeviceService.Core.Helpers.Common;
using DeviceService.Core.Helpers.Pagination;
using DeviceService.Core.Interfaces.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeviceService.Core.Repositories
{
    public class RoleManagementRepository : IRoleManagementRepository
    {
        private readonly RoleManager<Role> _roleManager;
        private readonly DeviceContext _dataContext;
        private readonly IGlobalRepository _globalRepository;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;

        public RoleManagementRepository(RoleManager<Role> roleManager, DeviceContext dataContext, IGlobalRepository globalRepository, IMapper mapper, UserManager<User> userManager)
        {
            _roleManager = roleManager;
            _dataContext = dataContext;
            _globalRepository = globalRepository;
            _mapper = mapper;
            _userManager = userManager;
        }

        public async Task<ReturnResponse> AssignRolesToUser(RoleUserAssignmentRequest roleAssignmentRequest)
        {
            if (roleAssignmentRequest.Users == null || roleAssignmentRequest.Roles == null || roleAssignmentRequest == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.ObjectNull,
                    StatusMessage = Utils.StatusMessageObjectNull
                };
            }

            //CHECK THE LIST OF ROLES TO ASSIGN FOR AUTHENTICITY
            //var listOfRolesToAssign = new List<string>();
            var listOfRolesToReturn = new List<Role>();

            foreach(var h in roleAssignmentRequest.Roles)
            {
                var roleDetail = await _roleManager.FindByIdAsync(Convert.ToString(h.Id));
                if (roleDetail == null)
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.NotFound,
                        StatusMessage = Utils.StatusMessageNotFound
                    };
                }

                //listOfRolesToAssign.Add(roleDetail.Name);
                listOfRolesToReturn.Add(roleDetail);
            }

            var userRolesToReturn = new List<UserAndRolesResponse>();

            foreach (var z in roleAssignmentRequest.Users)
            {
                var userDetail = await _userManager.FindByIdAsync(Convert.ToString(z.Id));
                if (userDetail == null)
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.NotFound,
                        StatusMessage = Utils.StatusMessageNotFound
                    };
                }

                //DELETE THE USER'S OLD ROLES
                var usersRoles = await _userManager.GetRolesAsync(userDetail);
                var iResult = await _userManager.RemoveFromRolesAsync(userDetail, usersRoles.AsEnumerable());
                if (!iResult.Succeeded)
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.NotSucceeded,
                        StatusMessage = Utils.StatusMessageNotSucceeded
                    };
                }

                var listOfRolesToAssign = listOfRolesToReturn.Select(a => a.Name);
                //UPDATE THE USER'S ROLES WITH THIS CURRENT INCOMING ROLES

                try
                {
                    var result = await _userManager.AddToRolesAsync(userDetail, listOfRolesToAssign);
                    if (!result.Succeeded)
                    {
                        return new ReturnResponse()
                        {
                            StatusCode = Utils.NotSucceeded,
                            StatusMessage = Utils.StatusMessageNotSucceeded
                        };
                    }
                }
                catch (Exception)
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.RoleAssignmentError,
                        StatusMessage = Utils.StatusMessageRoleAssignmentError
                    };
                }
                

                userRolesToReturn.Add(new UserAndRolesResponse()
                {
                    User = _mapper.Map<UserToReturn>(userDetail),
                    Roles = _mapper.Map<List<RoleResponse>>(listOfRolesToReturn)
                });
            }

            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                ObjectValue = userRolesToReturn,
                StatusMessage = Utils.StatusMessageSuccess
            };
        }

        public async Task<ReturnResponse> CreateFunctionality(List<Functionality> functionalities)
        {
            if (functionalities == null || functionalities.Any(a => string.IsNullOrWhiteSpace(a.FunctionalityName)))
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.ObjectNull,
                    StatusMessage = Utils.StatusMessageObjectNull
                };
            }

            foreach (var t in functionalities)
            {
                var projectModule = await _globalRepository.Get<ProjectModule>(t.ProjectModuleId);
                if(projectModule == null)
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.NotFound,
                        StatusMessage = Utils.StatusMessageNotFound
                    };
                }
            }

            var result = await _globalRepository.Add(functionalities);
            if (!result)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotSucceeded,
                    StatusMessage = Utils.StatusMessageNotSucceeded
                };
            }

            var saveVal = await _globalRepository.SaveAll();
            if (!saveVal.HasValue)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.SaveError,
                    StatusMessage = Utils.StatusMessageSaveError
                };
            }

            if (!saveVal.Value)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.SaveNoRowAffected,
                    StatusMessage = Utils.StatusMessageSaveNoRowAffected
                };
            }

            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                ObjectValue = functionalities,
                StatusMessage = Utils.StatusMessageSuccess
            };
        }

        public async Task<ReturnResponse> CreateProjectModule(List<ProjectModule> projectModules)
        {
            if (projectModules == null || projectModules.Any(a => string.IsNullOrWhiteSpace(a.ProjectModuleName)))
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.ObjectNull,
                    StatusMessage = Utils.StatusMessageObjectNull
                };
            }

            var projectModulesToReturn = new List<ProjectModule>();
            
            var result = await _globalRepository.Add(projectModules);
            if(!result)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotSucceeded,
                    StatusMessage = Utils.StatusMessageNotSucceeded
                };
            }
            
            var saveResult = await _globalRepository.SaveAll();
            if(saveResult.HasValue)
            {
                if(!saveResult.Value)
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.SaveNoRowAffected,
                        StatusMessage = Utils.StatusMessageSaveNoRowAffected
                    };
                }

                return new ReturnResponse()
                {
                    StatusCode = Utils.Success,
                    ObjectValue = projectModules,
                    StatusMessage = Utils.StatusMessageSuccess
                };
            }

            return new ReturnResponse()
            {
                StatusCode = Utils.SaveError,
                StatusMessage = Utils.StatusMessageSaveError
            };
        }

        public async Task<ReturnResponse> CreateRoles(List<Role> roles)
        {
            if (roles == null || roles.Any(a => string.IsNullOrWhiteSpace(a.Name)))
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.ObjectNull,
                    StatusMessage = Utils.StatusMessageObjectNull
                };
            }

            var rolesToReturn = new List<Role>();
            foreach (var t in roles)
            {
                if(t.UserType != Utils.UserType_User)
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.InvalidUserType,
                        StatusMessage = Utils.StatusMessageInvalidUserType
                    };
                }

                t.RoleName = t.Name;
                var result = await _roleManager.CreateAsync(t);
                if (!result.Succeeded)
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.NotSucceeded,
                        StatusMessage = Utils.StatusMessageNotSucceeded
                    };
                }

                rolesToReturn.Add(t);
            }

            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                ObjectValue = rolesToReturn,
                StatusMessage = Utils.StatusMessageSuccess
            };
        }

        public async Task<ReturnResponse> DeleteFunctionality(List<FunctionalityResponse> functionalities)
        {
            if (functionalities == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.ObjectNull,
                    StatusMessage = Utils.StatusMessageObjectNull
                };
            }

            var functionalitiesToReturn = new List<Functionality>();
            foreach (var t in functionalities)
            {
                var functionalityDetail = await _globalRepository.Get<Functionality>(t.FunctionalityId);
                if(functionalityDetail == null)
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.NotFound,
                        StatusMessage = Utils.StatusMessageNotFound
                    };
                }

                functionalitiesToReturn.Add(functionalityDetail);
            }

            _globalRepository.Delete(functionalitiesToReturn);
            var saveVal = await _globalRepository.SaveAll();
            if (!saveVal.HasValue)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.SaveError,
                    StatusMessage = Utils.StatusMessageSaveError
                };
            }

            if (!saveVal.Value)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.SaveNoRowAffected,
                    StatusMessage = Utils.StatusMessageSaveNoRowAffected
                };
            }

            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                ObjectValue = functionalitiesToReturn,
                StatusMessage = Utils.StatusMessageSuccess
            };
        }

        public async Task<ReturnResponse> DeleteProjectModule(List<ProjectModuleResponse> projectModules)
        {
            if (projectModules == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.ObjectNull,
                    StatusMessage = Utils.StatusMessageObjectNull
                };
            }

            var projectModulesToReturn = new List<ProjectModule>();
            foreach (var t in projectModules)
            {
                var projectModuleDetail = await _globalRepository.Get<ProjectModule>(t.ProjectModuleId);
                if(projectModuleDetail == null)
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.NotFound,
                        StatusMessage = Utils.StatusMessageNotFound
                    };
                }

                projectModulesToReturn.Add(projectModuleDetail);
            }

            _globalRepository.Delete(projectModulesToReturn);
            var saveVal = await _globalRepository.SaveAll();
            if (!saveVal.HasValue)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.SaveError,
                    StatusMessage = Utils.StatusMessageSaveError
                };
            }

            if (!saveVal.Value)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.SaveNoRowAffected,
                    StatusMessage = Utils.StatusMessageSaveNoRowAffected
                };
            }

            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                ObjectValue = projectModulesToReturn,
                StatusMessage = Utils.StatusMessageSuccess
            };
        }

        public async Task<ReturnResponse> DeleteRoles(List<RoleResponse> roles)
        {
            if (roles == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.ObjectNull,
                    StatusMessage = Utils.StatusMessageObjectNull
                };
            }

            var rolesToReturn = new List<Role>();
            foreach (var t in roles)
            {
                var roleDetail = await _roleManager.FindByIdAsync(Convert.ToString(t.Id));
                if(roleDetail == null)
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.NotFound,
                        StatusMessage = Utils.StatusMessageNotFound
                    };
                }

                var result = await _roleManager.DeleteAsync(roleDetail);
                if (!result.Succeeded)
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.NotSucceeded,
                        StatusMessage = Utils.StatusMessageNotSucceeded
                    };
                }

                rolesToReturn.Add(roleDetail);
            }

            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                ObjectValue = rolesToReturn,
                StatusMessage = Utils.StatusMessageSuccess
            };
        }

        public async Task<ReturnResponse> GetFunctionalities(UserParams userParams)
        {
            var functionalities = _dataContext.Functionality;

            var functionalitiesToReturn = await PagedList<Functionality>.CreateAsync(functionalities, userParams.PageNumber, userParams.PageSize);

            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                ObjectValue = functionalitiesToReturn,
                StatusMessage = Utils.StatusMessageSuccess
            };
        }

        public async Task<ReturnResponse> GetFunctionalities(int functionalityId)
        {
            var functionalities = await _dataContext.Functionality.Where(a => a.FunctionalityId == functionalityId).FirstOrDefaultAsync();
            if (functionalities == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotFound,
                    StatusMessage = Utils.StatusMessageNotFound
                };
            }

            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                ObjectValue = functionalities,
                StatusMessage = Utils.StatusMessageSuccess
            };
        }

        public async Task<ReturnResponse> GetProjectModules(UserParams userParams)
        {
            var projectModules = _dataContext.ProjectModule.Include(a => a.Functionalities);

            var projectModulesToReturn = await PagedList<ProjectModule>.CreateAsync(projectModules, userParams.PageNumber, userParams.PageSize);

            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                ObjectValue = projectModulesToReturn,
                StatusMessage = Utils.StatusMessageSuccess
            };
        }

        public async Task<ReturnResponse> GetProjectModules(int projectModuleId)
        {
            var projectModules = await _dataContext.ProjectModule.Where(a => a.ProjectModuleId == projectModuleId).Include(b => b.Functionalities).FirstOrDefaultAsync();
            if (projectModules == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotFound,
                    StatusMessage = Utils.StatusMessageNotFound
                };
            }

            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                ObjectValue = projectModules,
                StatusMessage = Utils.StatusMessageSuccess
            };
        }

        public async Task<ReturnResponse> GetRoles(UserParams userParams)
        {
            var roles = _roleManager.Roles.Include(a => a.UserRoles);

            var rolesToReturn = await PagedList<Role>.CreateAsync(roles, userParams.PageNumber, userParams.PageSize);

            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                ObjectValue = rolesToReturn,
                StatusMessage = Utils.StatusMessageSuccess
            };
        }

        public async Task<ReturnResponse> GetRoles(int id)
        {
            var role = await _roleManager.Roles.Where(c => c.Id == id).Include(a => a.UserRoles).FirstOrDefaultAsync();

            if (role == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotFound,
                    StatusMessage = Utils.StatusMessageNotFound
                };
            }

            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                ObjectValue = role,
                StatusMessage = Utils.StatusMessageSuccess
            };
        }

        public async Task<ReturnResponse> GetUsersRoles(UserParams userParams)
        {
            var usersRoles = _dataContext.UserRoles.Include(a => a.User).Include(d => d.Role);

            if (usersRoles == null || !(await usersRoles.AnyAsync()))
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotFound,
                    StatusMessage = Utils.StatusMessageNotFound
                };
            }

            var usersRolesToReturn = await PagedList<UserRole>.CreateAsync(usersRoles, userParams.PageNumber, userParams.PageSize);

            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                ObjectValue = usersRolesToReturn,
                StatusMessage = Utils.StatusMessageSuccess
            };
        }

        public async Task<ReturnResponse> UpdateRoles(List<RoleResponse> roles)
        {
            if (roles == null || roles.Any(a => string.IsNullOrWhiteSpace(a.Name)))
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.ObjectNull,
                    StatusMessage = Utils.StatusMessageObjectNull
                };
            }

            var rolesToReturn = new List<Role>();
            foreach (var t in roles)
            {
                if (t.UserType != Utils.UserType_User)
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.InvalidUserType,
                        StatusMessage = Utils.StatusMessageInvalidUserType
                    };
                }

                var roleDetail = await _roleManager.FindByIdAsync(Convert.ToString(t.Id));
                if (roleDetail == null)
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.NotFound,
                        StatusMessage = Utils.StatusMessageNotFound
                    };
                }

                //var roleToUpdate = _mapper.Map(t, roleDetail);
                //roleToUpdate.ModifiedAt = DateTimeOffset.Now;
                roleDetail.Name = t.Name;
                roleDetail.RoleDescription = t.RoleDescription;
                roleDetail.UserType = t.UserType;
                roleDetail.RoleName = t.Name;
                roleDetail.ModifiedAt = DateTimeOffset.Now;

                var result = await _roleManager.UpdateAsync(roleDetail);
                if (!result.Succeeded)
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.NotSucceeded,
                        StatusMessage = Utils.StatusMessageNotSucceeded
                    };
                }
                
                rolesToReturn.Add(roleDetail);
            }

            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                ObjectValue = rolesToReturn,
                StatusMessage = Utils.StatusMessageSuccess
            };
        }

        public async Task<ReturnResponse> AssignRolesToFunctionality(List<RoleFunctionalityAssignmentRequest> roleFunctionalityAssignmentRequest)
        {
            if (roleFunctionalityAssignmentRequest.Any(a => a.Roles == null) || roleFunctionalityAssignmentRequest.Any(b => b.Functionality == null) || roleFunctionalityAssignmentRequest == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.ObjectNull,
                    StatusMessage = Utils.StatusMessageObjectNull
                };
            }

            var functionalityRolesToReturn = new List<FunctionalityRole>();

            foreach (var z in roleFunctionalityAssignmentRequest)
            {
                var functionalityDetail = await _globalRepository.Get<Functionality>(z.Functionality.FunctionalityId);
                if (functionalityDetail == null)
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.NotFound,
                        StatusMessage = Utils.StatusMessageNotFound
                    };
                }

                foreach (var t in z.Roles)
                {
                    var roleDetail = await _roleManager.FindByIdAsync(Convert.ToString(t.Id));

                    if ((await FunctionalityRoleExists(functionalityDetail.FunctionalityName, roleDetail.Name)))
                    {
                        return new ReturnResponse()
                        {
                            StatusCode = Utils.ObjectExists,
                            StatusMessage = Utils.StatusMessageObjectExists
                        };
                    }

                    var functionalityRoleDetail = new FunctionalityRole()
                    {
                        FunctionalityName = functionalityDetail.FunctionalityName,
                        RoleId = roleDetail.Id,
                        RoleName = roleDetail.Name
                    };
                    _globalRepository.Add(functionalityRoleDetail);

                    var saveVal = await _globalRepository.SaveAll();
                    if (saveVal == null)
                    {
                        return new ReturnResponse()
                        {
                            StatusCode = Utils.SaveError,
                            StatusMessage = Utils.StatusMessageSaveError
                        };
                    }

                    if (!saveVal.Value)
                    {
                        return new ReturnResponse()
                        {
                            StatusCode = Utils.SaveNoRowAffected,
                            StatusMessage = Utils.StatusMessageSaveNoRowAffected
                        };
                    }

                    functionalityRolesToReturn.Add(functionalityRoleDetail);
                }
            }

            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                StatusMessage = Utils.StatusMessageSuccess,
                ObjectValue = functionalityRolesToReturn
            };
        }

        public async Task<ReturnResponse> GetFunctionalitiesRoles(UserParams userParams)
        {
            var functionalitiesRoles = _dataContext.FunctionalityRole;

            var functionalityRolesToReturn = await PagedList<FunctionalityRole>.CreateAsync(functionalitiesRoles, userParams.PageNumber, userParams.PageSize);

            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                ObjectValue = functionalityRolesToReturn,
                StatusMessage = Utils.StatusMessageSuccess
            };
        }

        private async Task<bool> FunctionalityRoleExists(string functionalityName, string roleName)
        {
            return (await _dataContext.FunctionalityRole.AnyAsync(a => (a.FunctionalityName == functionalityName) && (a.RoleName == roleName)));
        }
    }
}
