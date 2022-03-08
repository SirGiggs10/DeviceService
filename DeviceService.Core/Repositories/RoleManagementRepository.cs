using AutoMapper;
using DeviceService.Core.Data.DataContext;
using DeviceService.Core.Dtos.Global;
using DeviceService.Core.Helpers.Common;
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
                foreach (var roleDett in listOfRolesToAssign)
                {
                    //CHECK TO SEE IF ANYBODY HOLDS THAT ROLE (APART FROM AllStaff ROLE)...IF ANY OVERWRITE IT
                    if (roleDett.Equals(Utils.MdCeoBranchManagerRole, StringComparison.OrdinalIgnoreCase) || roleDett.Equals(Utils.HeadOfDepartmentRole, StringComparison.OrdinalIgnoreCase) || roleDett.Equals(Utils.HeadOfUnitRole, StringComparison.OrdinalIgnoreCase) || roleDett.Equals(Utils.ZonalManagerRole, StringComparison.OrdinalIgnoreCase))
                    {
                        var allUsersInRole = await _userManager.GetUsersInRoleAsync(roleDett);
                        /*if (allUsersInRole != null && allUsersInRole.Any())
                        {
                            
                        }*/
                        if (roleDett.Equals(Utils.HeadOfDepartmentRole, StringComparison.OrdinalIgnoreCase))
                        {
                            var userDept = await _dataContext.Staff.Where(a => a.StaffId == userDetail.UserTypeId).Include(b => b.Department).FirstOrDefaultAsync();
                            if (userDept == null)
                            {
                                return new ReturnResponse()
                                {
                                    StatusCode = Utils.NotFound,
                                    StatusMessage = Utils.StatusMessageNotFound
                                };
                            }

                            foreach (var uss in allUsersInRole)
                            {
                                var currUserWithRole = await _dataContext.Staff.FindAsync(uss.UserTypeId);
                                if (currUserWithRole == null)
                                {
                                    return new ReturnResponse()
                                    {
                                        StatusCode = Utils.NotFound,
                                        StatusMessage = Utils.StatusMessageNotFound
                                    };
                                }

                                if (userDept.DepartmentId == currUserWithRole.DepartmentId)
                                {
                                    var fResult = await _userManager.RemoveFromRoleAsync(uss, roleDett);
                                    if (!fResult.Succeeded)
                                    {
                                        return new ReturnResponse()
                                        {
                                            StatusCode = Utils.NotSucceeded,
                                            StatusMessage = Utils.StatusMessageNotSucceeded
                                        };
                                    }
                                }
                            }

                            //THEN UPDATE DEPARTMENT HEAD OF DEPARTMENT
                            var departmentToUpdate = userDept.Department;
                            departmentToUpdate.HeadOfDepartmentStaffId = userDept.StaffId;
                            _globalRepository.Update(departmentToUpdate);
                            var deptResult = await _globalRepository.SaveAll();
                            if (deptResult.HasValue)
                            {
                                if (!deptResult.Value)
                                {
                                    return new ReturnResponse()
                                    {
                                        StatusCode = Utils.SaveNoRowAffected,
                                        StatusMessage = Utils.StatusMessageSaveNoRowAffected
                                    };
                                }
                            }
                            else
                            {
                                return new ReturnResponse()
                                {
                                    StatusCode = Utils.SaveError,
                                    StatusMessage = Utils.StatusMessageSaveError
                                };
                            }
                        }
                        else if (roleDett.Equals(Utils.HeadOfUnitRole, StringComparison.OrdinalIgnoreCase))
                        {
                            var userUnit = await _dataContext.Staff.Where(a => a.StaffId == userDetail.UserTypeId).Include(b => b.SubUnit).FirstOrDefaultAsync();
                            if (userUnit == null)
                            {
                                return new ReturnResponse()
                                {
                                    StatusCode = Utils.NotFound,
                                    StatusMessage = Utils.StatusMessageNotFound
                                };
                            }

                            foreach (var uss in allUsersInRole)
                            {
                                var currUserWithRole = await _dataContext.Staff.FindAsync(uss.UserTypeId);
                                if (currUserWithRole == null)
                                {
                                    return new ReturnResponse()
                                    {
                                        StatusCode = Utils.NotFound,
                                        StatusMessage = Utils.StatusMessageNotFound
                                    };
                                }

                                if (userUnit.SubUnitId == currUserWithRole.SubUnitId)
                                {
                                    var fResult = await _userManager.RemoveFromRoleAsync(uss, roleDett);
                                    if (!fResult.Succeeded)
                                    {
                                        return new ReturnResponse()
                                        {
                                            StatusCode = Utils.NotSucceeded,
                                            StatusMessage = Utils.StatusMessageNotSucceeded
                                        };
                                    }
                                }
                            }

                            //THEN UPDATE SUBUNIT HEAD OF UNIT
                            var subUnitToUpdate = userUnit.SubUnit;
                            subUnitToUpdate.HeadOfUnitStaffId = userUnit.StaffId;
                            _globalRepository.Update(subUnitToUpdate);
                            var deptResult = await _globalRepository.SaveAll();
                            if (deptResult.HasValue)
                            {
                                if (!deptResult.Value)
                                {
                                    return new ReturnResponse()
                                    {
                                        StatusCode = Utils.SaveNoRowAffected,
                                        StatusMessage = Utils.StatusMessageSaveNoRowAffected
                                    };
                                }
                            }
                            else
                            {
                                return new ReturnResponse()
                                {
                                    StatusCode = Utils.SaveError,
                                    StatusMessage = Utils.StatusMessageSaveError
                                };
                            }
                        }
                        else if (roleDett.Equals(Utils.ZonalManagerRole, StringComparison.OrdinalIgnoreCase))
                        {
                            //TO BE UPDATED LATER AFTER UI HAS BEEN DONE FOR IT
                        }
                        else
                        {
                            //MD/CEO/BRANCHMANAGER ROLE
                            foreach (var ur in allUsersInRole)
                            {
                                var idResult = await _userManager.RemoveFromRoleAsync(ur, roleDett);
                                if (!idResult.Succeeded)
                                {
                                    return new ReturnResponse()
                                    {
                                        StatusCode = Utils.NotSucceeded,
                                        StatusMessage = Utils.StatusMessageNotSucceeded
                                    };
                                }
                            }
                        }
                    }
                }

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
            /*
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
            */
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
                if(t.UserType != Utils.Staff && t.UserType != Utils.Customer)
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.InvalidUserType,
                        StatusMessage = Utils.StatusMessageInvalidUserType
                    };
                }

                if(t.UserType == Utils.Staff)
                {
                    var supportLevel = await _globalRepository.Get<SupportLevel>(t.SupportLevelId);
                    if (supportLevel == null)
                    {
                        return new ReturnResponse()
                        {
                            StatusCode = Utils.NotFound,
                            StatusMessage = Utils.StatusMessageNotFound
                        };
                    }
                }
                else
                {
                    //CUSTOMER
                    t.SupportLevelId = Utils.NoSupportLevel;
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
            if(functionalities == null || !(await functionalities.AnyAsync()))
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotFound,
                    StatusMessage = Utils.StatusMessageNotFound
                };
            }

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

            if (projectModules == null || !projectModules.Any())
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotFound,
                    StatusMessage = Utils.StatusMessageNotFound
                };
            }

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
            var roles = _roleManager.Roles.Include(a => a.UserRoles).Include(b => b.SupportLevel);

            if (roles == null || !roles.Any())
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotFound,
                    StatusMessage = Utils.StatusMessageNotFound
                };
            }

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
            var role = await _roleManager.Roles.Where(c => c.Id == id).Include(a => a.UserRoles).Include(b => b.SupportLevel).FirstOrDefaultAsync();

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

        public async Task<ReturnResponse> GetStaffUsersRoles(UserParams userParams)
        {
            var staffUsersRoles = _dataContext.UserRoles.Include(a => a.User).Where(b => b.User.UserType == Utils.Staff).Include(a => a.User).ThenInclude(c => c.Staff).Include(d => d.Role);

            if (staffUsersRoles == null || !(await staffUsersRoles.AnyAsync()))
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotFound,
                    StatusMessage = Utils.StatusMessageNotFound
                };
            }

            var staffUsersRolesToReturn = await PagedList<UserRole>.CreateAsync(staffUsersRoles, userParams.PageNumber, userParams.PageSize);

            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                ObjectValue = staffUsersRolesToReturn,
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
                if (t.UserType != Utils.Staff && t.UserType != Utils.Customer)
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.InvalidUserType,
                        StatusMessage = Utils.StatusMessageInvalidUserType
                    };
                }

                var supportLevel = await _globalRepository.Get<SupportLevel>(t.SupportLevelId);
                if (supportLevel == null)
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.NotFound,
                        StatusMessage = Utils.StatusMessageNotFound
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
                roleDetail.SupportLevelId = t.SupportLevelId;
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

     }
}
