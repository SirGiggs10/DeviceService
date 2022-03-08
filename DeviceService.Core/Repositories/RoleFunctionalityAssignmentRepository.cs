/*
using Ayuda_Help_Desk.Data;
using Ayuda_Help_Desk.Dtos;
using Ayuda_Help_Desk.Dtos.General;
using Ayuda_Help_Desk.Dtos.RoleFunctionality;
using Ayuda_Help_Desk.Helpers;
using Ayuda_Help_Desk.Interface;
using Ayuda_Help_Desk.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ayuda_Help_Desk.Repositories
{
    public class RoleFunctionalityAssignmentRepository : IRoleFunctionalityAssignmentRepository
    {
        private readonly UserManager<User> _userManager;
        private readonly DataContext _dataContext;
        private readonly RoleManager<Role> _roleManager;
        private readonly IGlobalRepository _globalRepository;

        public RoleFunctionalityAssignmentRepository(DataContext dataContext, UserManager<User> userManager, RoleManager<Role> roleManager, IGlobalRepository globalRepository)
        {
            _dataContext = dataContext;
            _userManager = userManager;
            _roleManager = roleManager;
            _globalRepository = globalRepository;
        }

        public async Task<ReturnResponse> GetUsers()
        {
            var users = await _userManager.Users.Where(a => a.UserType == Utils.Staff).Include(d => d.UserRoles).ThenInclude(e => e.Role).ToListAsync();

            if(users == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotFound
                };
            }

            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                ObjectValue = users
            };
        }

        public async Task<ReturnResponse> GetUsers(int id)
        {
            var user = await _userManager.FindByIdAsync(Convert.ToString(id));

            if (user == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotFound
                };
            }

            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                ObjectValue = user
            };
        }

        public async Task<ReturnResponse> CreateRoles(List<Role> roles)
        {
            if(roles == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = -1
                };
            }

            var rolesToReturn = new List<Role>();
            var dbTransaction = await _dataContext.Database.BeginTransactionAsync();
            foreach(var t in roles)
            {
                var result = await _roleManager.CreateAsync(t);
                if(!result.Succeeded)
                {
                    await dbTransaction.RollbackAsync();
                    return new ReturnResponse()
                    {
                        StatusCode = 1
                    };
                }

                rolesToReturn.Add(t);
            }

            await dbTransaction.CommitAsync();
            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                ObjectValue = rolesToReturn
            };
        }

        public async Task<ReturnResponse> GetRoles()
        {
            var roles = await _roleManager.Roles.ToListAsync();

            if (roles == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotFound
                };
            }

            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                ObjectValue = roles
            };
        }

        public async Task<ReturnResponse> GetRoleByName(string name)
        {
            var roles = await _roleManager.FindByNameAsync(name);

            if (roles == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotFound
                };
            }

            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                ObjectValue = roles
            };
        }

        public async Task<ReturnResponse> DeleteRoles(List<RoleResponse> roles)
        {
            if (roles == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = -1
                };
            }

            var rolesToReturn = new List<Role>();
            var dbTransaction = await _dataContext.Database.BeginTransactionAsync();
            foreach (var t in roles)
            {
                var roleDetail = await _roleManager.FindByIdAsync(Convert.ToString(t.Id));
                var result = await _roleManager.DeleteAsync(roleDetail);
                if (!result.Succeeded)
                {
                    await dbTransaction.RollbackAsync();
                    return new ReturnResponse()
                    {
                        StatusCode = 1
                    };
                }

                rolesToReturn.Add(roleDetail);
            }

            await dbTransaction.CommitAsync();
            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                ObjectValue = rolesToReturn
            };
        }

        public async Task<ReturnResponse> AssignRolesToUser(List<RoleUserAssignmentRequest> roleAssignmentRequest)
        {
            if (roleAssignmentRequest.Any(a => a.Roles == null) || roleAssignmentRequest.Any(a => a.User == null) || roleAssignmentRequest == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = -1
                };
            }            

            var dbTransaction = await _dataContext.Database.BeginTransactionAsync();
            var userRolesToReturn = new List<UserRole>();

            foreach(var z in roleAssignmentRequest)
            {
                var userDetail = await _userManager.FindByIdAsync(Convert.ToString(z.User.Id));
                if (userDetail == null)
                {
                    await dbTransaction.RollbackAsync();
                    return new ReturnResponse()
                    {                        
                        StatusCode = Utils.NotFound
                    };
                }

                //DELETE THE USER'S OLD ROLES AND UPDATE WITH THIS CURRENT INCOMING ROLES
                var usersRoles = await _userManager.GetRolesAsync(userDetail);
                var iResult = await _userManager.RemoveFromRolesAsync(userDetail, usersRoles.AsEnumerable());
                if(!iResult.Succeeded)
                {
                    await dbTransaction.RollbackAsync();
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.NotSucceeded
                    };
                }

                foreach (var t in z.Roles)
                {
                    var roleDetail = await _roleManager.FindByIdAsync(Convert.ToString(t.Id));
                    if ((await UserRoleExists(userDetail.Id, roleDetail.Id)))
                    {
                        await dbTransaction.RollbackAsync();
                        return new ReturnResponse()
                        {
                            StatusCode = Utils.ObjectExists
                        };
                    }

                    //CHECK TO SEE IF ANYBODY HOLDS THAT ROLE (APART FROM AllStaff ROLE)...IF ANY OVERWRITE IT
                    var roleDett = roleDetail.Name.ToLower();
                    if (roleDett != "allstaff")
                    {
                        var allUsersInRole = await _userManager.GetUsersInRoleAsync(roleDetail.Name);
                        if (allUsersInRole != null && allUsersInRole.Any())
                        {
                            if (roleDett == "hods")
                            {
                                var userDept = await _dataContext.Staff.FindAsync(userDetail.UserTypeId);
                                if(userDept == null)
                                {
                                    await dbTransaction.RollbackAsync();
                                    return new ReturnResponse()
                                    {
                                        StatusCode = Utils.NotFound
                                    };
                                }

                                foreach(var uss in allUsersInRole)
                                {
                                    var currUserWithRole = await _dataContext.Staff.FindAsync(uss.UserTypeId);
                                    if (currUserWithRole == null)
                                    {
                                        await dbTransaction.RollbackAsync();
                                        return new ReturnResponse()
                                        {
                                            StatusCode = Utils.NotFound
                                        };
                                    }

                                    if (userDept.DepartmentId == currUserWithRole.DepartmentId)
                                    {
                                        var fResult = await _userManager.RemoveFromRoleAsync(uss, roleDetail.Name);
                                        if(!fResult.Succeeded)
                                        {
                                            await dbTransaction.RollbackAsync();
                                            return new ReturnResponse()
                                            {
                                                StatusCode = Utils.NotSucceeded
                                            };
                                        }
                                    }
                                }
                            }
                            else
                            {
                                foreach (var ur in allUsersInRole)
                                {
                                    var idResult = await _userManager.RemoveFromRoleAsync(ur, roleDetail.Name);
                                    if (!idResult.Succeeded)
                                    {
                                        await dbTransaction.RollbackAsync();
                                        return new ReturnResponse()
                                        {
                                            StatusCode = Utils.NotSucceeded
                                        };
                                    }
                                }
                            }          
                        }       
                    }

                    var result = await _userManager.AddToRoleAsync(userDetail, roleDetail.Name);
                    if (!result.Succeeded)
                    {
                        await dbTransaction.RollbackAsync();
                        return new ReturnResponse()
                        {
                            StatusCode = 1
                        };
                    }

                    userRolesToReturn.Add(new UserRole()
                    {
                        User = userDetail,
                        Role = roleDetail
                    });
                }
            }

            await dbTransaction.CommitAsync();
            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                ObjectValue = userRolesToReturn
            };
        }

        public async Task<ReturnResponse> GetUsersRoles()
        {
            var usersRoles = await _dataContext.UserRoles.ToListAsync();

            if (usersRoles == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotFound
                };
            }

            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                ObjectValue = usersRoles
            };
        }

        public async Task<ReturnResponse> CreateProjectModule(List<ProjectModule> projectModules)
        {
            if (projectModules == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = -1
                };
            }

            var projectModulesToReturn = new List<ProjectModule>();
            var dbTransaction = await _dataContext.Database.BeginTransactionAsync();
            foreach (var t in projectModules)
            {
                _globalRepository.Add(t);
                var saveVal = await _globalRepository.SaveAll();
                if (saveVal == null)
                {
                    await dbTransaction.RollbackAsync();
                    return new ReturnResponse()
                    {
                        StatusCode = 1
                    };
                }

                if (!saveVal.Value)
                {
                    await dbTransaction.RollbackAsync();
                    return new ReturnResponse()
                    {
                        StatusCode = 2
                    };
                }
                projectModulesToReturn.Add(t);
            }

            await dbTransaction.CommitAsync();
            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                ObjectValue = projectModulesToReturn
            };
        }

        public async Task<ReturnResponse> GetProjectModules()
        {
            var projectModules = await _dataContext.ProjectModule.Include(a => a.Functionalities).ToListAsync();

            if (projectModules == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotFound
                };
            }

            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                ObjectValue = projectModules
            };
        }

        public async Task<ReturnResponse> DeleteProjectModule(List<ProjectModuleResponse> projectModules)
        {
            if (projectModules == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = -1
                };
            }

            var projectModulesToReturn = new List<ProjectModule>();
            var dbTransaction = await _dataContext.Database.BeginTransactionAsync();
            foreach (var t in projectModules)
            {
                var projectModuleDetail = await _globalRepository.Get<ProjectModule>(t.ProjectModuleId);
                _globalRepository.Delete(projectModuleDetail);
                var saveVal = await _globalRepository.SaveAll();
                if (saveVal == null)
                {
                    await dbTransaction.RollbackAsync();
                    return new ReturnResponse()
                    {
                        StatusCode = 1
                    };
                }

                if (!saveVal.Value)
                {
                    await dbTransaction.RollbackAsync();
                    return new ReturnResponse()
                    {
                        StatusCode = 2
                    };
                }

                projectModulesToReturn.Add(projectModuleDetail);
            }

            await dbTransaction.CommitAsync();
            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                ObjectValue = projectModulesToReturn
            };
        }

        public async Task<ReturnResponse> CreateFunctionality(List<Functionality> functionalities)
        {
            if (functionalities == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = -1
                };
            }

            var functionalitiesToReturn = new List<Functionality>();
            var dbTransaction = await _dataContext.Database.BeginTransactionAsync();
            foreach (var t in functionalities)
            {
                _globalRepository.Add(t);
                var saveVal = await _globalRepository.SaveAll();
                if (saveVal == null)
                {
                    await dbTransaction.RollbackAsync();
                    return new ReturnResponse()
                    {
                        StatusCode = 1
                    };
                }

                if(!saveVal.Value)
                {
                    await dbTransaction.RollbackAsync();
                    return new ReturnResponse()
                    {
                        StatusCode = 2
                    };
                }
                functionalitiesToReturn.Add(t);
            }

            await dbTransaction.CommitAsync();
            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                ObjectValue = functionalitiesToReturn
            };
        }

        public async Task<ReturnResponse> GetFunctionalities()
        {
            var functionalities = await _dataContext.Functionality.ToListAsync();

            if (functionalities == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotFound
                };
            }

            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                ObjectValue = functionalities
            };
        }

        public async Task<ReturnResponse> DeleteFunctionality(List<FunctionalityResponse> functionalities)
        {
            if (functionalities == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = -1
                };
            }

            var functionalitiesToReturn = new List<Functionality>();
            var dbTransaction = await _dataContext.Database.BeginTransactionAsync();
            foreach (var t in functionalities)
            {
                var functionalityDetail = await _globalRepository.Get<Functionality>(t.FunctionalityId);
                _globalRepository.Delete(functionalityDetail);
                var saveVal = await _globalRepository.SaveAll();
                if (saveVal == null)
                {
                    await dbTransaction.RollbackAsync();
                    return new ReturnResponse()
                    {
                        StatusCode = 1
                    };
                }

                if (!saveVal.Value)
                {
                    await dbTransaction.RollbackAsync();
                    return new ReturnResponse()
                    {
                        StatusCode = 2
                    };
                }

                functionalitiesToReturn.Add(functionalityDetail);
            }

            await dbTransaction.CommitAsync();
            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                ObjectValue = functionalitiesToReturn
            };
        }

        public async Task<ReturnResponse> AssignRolesToFunctionality(List<RoleFunctionalityAssignmentRequest> roleFunctionalityAssignmentRequest)
        {
            if (roleFunctionalityAssignmentRequest.Any(a => a.Roles == null) || roleFunctionalityAssignmentRequest.Any(b => b.Functionality == null) || roleFunctionalityAssignmentRequest == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.ObjectNull
                };
            }

            var dbTransaction = await _dataContext.Database.BeginTransactionAsync();
            var functionalityRolesToReturn = new List<FunctionalityRole>();

            foreach(var z in roleFunctionalityAssignmentRequest)
            {
                var functionalityDetail = await _globalRepository.Get<Functionality>(z.Functionality.FunctionalityId);
                if (functionalityDetail == null)
                {
                    await dbTransaction.RollbackAsync();
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.NotFound
                    };
                }

                foreach (var t in z.Roles)
                {
                    var roleDetail = await _roleManager.FindByIdAsync(Convert.ToString(t.Id));

                    if ((await FunctionalityRoleExists(functionalityDetail.FunctionalityName, roleDetail.Name)))
                    {
                        await dbTransaction.RollbackAsync();
                        return new ReturnResponse()
                        {
                            StatusCode = Utils.ObjectExists
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
                        await dbTransaction.RollbackAsync();
                        return new ReturnResponse()
                        {
                            StatusCode = Utils.SaveError
                        };
                    }

                    if (!saveVal.Value)
                    {
                        await dbTransaction.RollbackAsync();
                        return new ReturnResponse()
                        {
                            StatusCode = Utils.SaveNoRowAffected
                        };
                    }

                    functionalityRolesToReturn.Add(functionalityRoleDetail);
                }
            }

            await dbTransaction.CommitAsync();
            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                ObjectValue = functionalityRolesToReturn
            };
        }

        public async Task<ReturnResponse> CreateRoleAndAssignFunctionalitiesToRole(List<FunctionalityRoleAssignmentRequest> functionalityRoleAssignmentRequests)
        {
            if (functionalityRoleAssignmentRequests.Any(b => b.Functionalities == null) || functionalityRoleAssignmentRequests.Any(c => c.RoleRequest == null) || functionalityRoleAssignmentRequests == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.ObjectNull
                };
            }
            
            var dbTransaction = await _dataContext.Database.BeginTransactionAsync();
            var functionalityRolesToReturn = new List<FunctionalityRole>();

            foreach (var z in functionalityRoleAssignmentRequests)
            {
                var role = new Role()
                {
                    Name = z.RoleRequest.Name,
                    RoleDescription = z.RoleRequest.RoleDescription
                };
                if (role == null)
                {
                    await dbTransaction.RollbackAsync();
                    return new ReturnResponse()
                    {
                        StatusCode = -1
                    };
                }

                var result = await _roleManager.CreateAsync(role);
                if (!result.Succeeded)
                {
                    await dbTransaction.RollbackAsync();
                    return new ReturnResponse()
                    {
                        StatusCode = 1
                    };
                }

                var roleDetail = role;

                foreach (var t in z.Functionalities)
                {
                    var functionalityDetail = await _dataContext.Functionality.FindAsync(t.FunctionalityId);

                    if ((await FunctionalityRoleExists(functionalityDetail.FunctionalityName, roleDetail.Name)))
                    {
                        await dbTransaction.RollbackAsync();
                        return new ReturnResponse()
                        {
                            StatusCode = Utils.ObjectExists
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
                        await dbTransaction.RollbackAsync();
                        return new ReturnResponse()
                        {
                            StatusCode = Utils.SaveError
                        };
                    }

                    if (!saveVal.Value)
                    {
                        await dbTransaction.RollbackAsync();
                        return new ReturnResponse()
                        {
                            StatusCode = Utils.SaveNoRowAffected
                        };
                    }

                    functionalityRolesToReturn.Add(functionalityRoleDetail);
                }
            }

            await dbTransaction.CommitAsync();
            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                ObjectValue = functionalityRolesToReturn
            };
        }

        public async Task<ReturnResponse> GetFunctionalitiesRoles()
        {
            var functionalitiesRoles = await _dataContext.FunctionalityRole.ToListAsync();

            if (functionalitiesRoles == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotFound
                };
            }

            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                ObjectValue = functionalitiesRoles
            };
        }

        private async Task<bool> UserRoleExists(int userId, int roleId)
        {
            return (await _dataContext.UserRoles.AnyAsync(a => (a.UserId == userId) && (a.RoleId == roleId)));
        }

        private async Task<bool> FunctionalityRoleExists(string functionalityName, string roleName)
        {
            return (await _dataContext.FunctionalityRole.AnyAsync(a => (a.FunctionalityName == functionalityName) && (a.RoleName == roleName)));
        }
    }
}
*/