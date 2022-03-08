using Ayuda_Help_Desk.Dtos.General;
using Ayuda_Help_Desk.Dtos.RoleFunctionality;
using Ayuda_Help_Desk.Helpers;
using Ayuda_Help_Desk.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ayuda_Help_Desk.Interfaces
{
    public interface IRoleManagementRepository
    {
        public Task<ReturnResponse> CreateRoles(List<Role> roles);
        public Task<ReturnResponse> GetRoles(UserParams userParams);
        public Task<ReturnResponse> GetRoles(int id);
        public Task<ReturnResponse> UpdateRoles(List<RoleResponse> roles);
        public Task<ReturnResponse> DeleteRoles(List<RoleResponse> roles);
        public Task<ReturnResponse> AssignRolesToUser(RoleUserAssignmentRequest roleAssignmentRequest);
        public Task<ReturnResponse> GetStaffUsersRoles(UserParams userParams);
        public Task<ReturnResponse> CreateProjectModule(List<ProjectModule> projectModules);
        public Task<ReturnResponse> GetProjectModules(UserParams userParams);
        public Task<ReturnResponse> GetProjectModules(int projectModuleId);
        public Task<ReturnResponse> DeleteProjectModule(List<ProjectModuleResponse> projectModules);
        public Task<ReturnResponse> CreateFunctionality(List<Functionality> functionalities);
        public Task<ReturnResponse> GetFunctionalities(UserParams userParams);
        public Task<ReturnResponse> GetFunctionalities(int functionalityId);
        public Task<ReturnResponse> DeleteFunctionality(List<FunctionalityResponse> functionalities);
    }
}
