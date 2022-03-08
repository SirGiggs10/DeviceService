/*
using Ayuda_Help_Desk.Dtos.General;
using Ayuda_Help_Desk.Dtos.RoleFunctionality;
using Ayuda_Help_Desk.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ayuda_Help_Desk.Interface
{
    public interface IRoleFunctionalityAssignmentRepository
    {
        public Task<ReturnResponse> GetUsers();
        public Task<ReturnResponse> GetUsers(int id);
        public Task<ReturnResponse> CreateRoles(List<Role> roles);
        public Task<ReturnResponse> GetRoles();
        public Task<ReturnResponse> DeleteRoles(List<RoleResponse> roles);
        public Task<ReturnResponse> AssignRolesToUser(List<RoleUserAssignmentRequest> roleAssignmentRequest);
        public Task<ReturnResponse> GetUsersRoles();
        public Task<ReturnResponse> GetRoleByName(string name);
        public Task<ReturnResponse> CreateProjectModule(List<ProjectModule> projectModules);
        public Task<ReturnResponse> GetProjectModules();
        public Task<ReturnResponse> DeleteProjectModule(List<ProjectModuleResponse> projectModules);
        public Task<ReturnResponse> CreateFunctionality(List<Functionality> functionalities);
        public Task<ReturnResponse> GetFunctionalities();
        public Task<ReturnResponse> DeleteFunctionality(List<FunctionalityResponse> functionalities);
        public Task<ReturnResponse> AssignRolesToFunctionality(List<RoleFunctionalityAssignmentRequest> roleFunctionalityAssignmentRequest);
        public Task<ReturnResponse> CreateRoleAndAssignFunctionalitiesToRole(List<FunctionalityRoleAssignmentRequest> functionalityRoleAssignmentRequests);
        public Task<ReturnResponse> GetFunctionalitiesRoles();
    }
}
*/