/*
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ayuda_Help_Desk.Data;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ayuda_Help_Desk.Helpers;
using Ayuda_Help_Desk.Models;
using Ayuda_Help_Desk.Interface;
using Ayuda_Help_Desk.Dtos.RoleFunctionality;
using Ayuda_Help_Desk.Dtos.Auth;

namespace Amana.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class RoleFunctionalityAssignmentController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly DataContext _dataContext;
        private readonly IRoleFunctionalityAssignmentRepository _userManagementRepository;
        private readonly IMapper _mapper;

        public RoleFunctionalityAssignmentController(DataContext dataContext, UserManager<User> userManager, IRoleFunctionalityAssignmentRepository userManagementRepository, IMapper mapper)
        {
            _dataContext = dataContext;
            _userManager = userManager;
            _userManagementRepository = userManagementRepository;
            _mapper = mapper;
        }

        [HttpGet("Users")]
        public async Task<ActionResult> GetUsers()
        {
            var result = await _userManagementRepository.GetUsers();
            
            if(result.StatusCode == Utils.Success)
            {
                result.ObjectValue = _mapper.Map<List<UserAndRoleResponse>>(((List<User>)result.ObjectValue));
                return StatusCode(StatusCodes.Status200OK, result.ObjectValue);
            }
            else if(result.StatusCode == Utils.NotFound)
            {
                return StatusCode(StatusCodes.Status404NotFound, result);
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, result);
            }
        }

        [HttpGet("Users/{id}")]
        public async Task<ActionResult> GetUser(int id)
        {
            var result = await _userManagementRepository.GetUsers(id);

            if (result.StatusCode == Utils.Success)
            {
                result.ObjectValue = _mapper.Map<UserToReturn>(((User)result.ObjectValue));
                return StatusCode(StatusCodes.Status200OK, result.ObjectValue);
            }
            else if (result.StatusCode == Utils.NotFound)
            {
                return StatusCode(StatusCodes.Status404NotFound, result);
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, result);
            }
        }

        [HttpPost("Roles/Create")]
        public async Task<ActionResult> PostRoles(List<RoleRequest> roles)
        {
            var result = await _userManagementRepository.CreateRoles(_mapper.Map<List<Role>>(roles));

            if (result.StatusCode == Utils.Success)
            {
                result.ObjectValue = _mapper.Map<List<RoleResponse>>(((List<Role>)result.ObjectValue));
                return StatusCode(StatusCodes.Status200OK, result.ObjectValue);
            }
            else if (result.StatusCode == Utils.NotFound)
            {
                return StatusCode(StatusCodes.Status404NotFound, result);
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, result);
            }
        }

        [HttpGet("Roles")]
        public async Task<ActionResult> GetRoles()
        {
            var result = await _userManagementRepository.GetRoles();

            if (result.StatusCode == Utils.Success)
            {
                result.ObjectValue = _mapper.Map<List<RoleResponse>>(((List<Role>)result.ObjectValue));
                return StatusCode(StatusCodes.Status200OK, result.ObjectValue);
            }
            else if (result.StatusCode == Utils.NotFound)
            {
                return StatusCode(StatusCodes.Status404NotFound, result);
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, result);
            }
        }

        [HttpPost("Roles/Delete")]
        public async Task<ActionResult> DeleteRoles(List<RoleResponse> roles)
        {
            var result = await _userManagementRepository.DeleteRoles(roles);

            if (result.StatusCode == Utils.Success)
            {
                result.ObjectValue = _mapper.Map<List<RoleResponse>>(((List<Role>)result.ObjectValue));
                return StatusCode(StatusCodes.Status200OK, result.ObjectValue);
            }
            else if (result.StatusCode == Utils.NotFound)
            {
                return StatusCode(StatusCodes.Status404NotFound, result);
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, result);
            }
        }

        [HttpPost("Users/Roles/Assign")]
        public async Task<ActionResult> PostAssignRolesToUser(List<RoleUserAssignmentRequest> roleAssignmentRequest)
        {
            var result = await _userManagementRepository.AssignRolesToUser(roleAssignmentRequest);

            if (result.StatusCode == Utils.Success)
            {
                result.ObjectValue = _mapper.Map<List<UserRoleResponse>>(((List<UserRole>)result.ObjectValue));
                return StatusCode(StatusCodes.Status200OK, result.ObjectValue);
            }
            else if (result.StatusCode == Utils.NotFound)
            {
                return StatusCode(StatusCodes.Status404NotFound, result);
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, result);
            }
        }

        [HttpPost("AssignRoleFunctionality")]
        public async Task<ActionResult> AssignRoleFunctionality(List<AssignRoleFunctionality> assignRoleFunctionalities)
        {
             foreach(var functionalityRole in assignRoleFunctionalities)
             {
                var roleFunctionalities = new List<RoleFunctionalityAssignmentRequest>();

                var Roles = new List<RoleResponse>();
                var Functionality = new FunctionalityResponse();
                var roleFunctionality = new RoleFunctionalityAssignmentRequest();
                if (functionalityRole.Roles != "")
                {
                    Functionality = _mapper.Map<FunctionalityResponse>(await _dataContext.Functionality.Where(a => a.FunctionalityName == functionalityRole.field1).FirstOrDefaultAsync());
                    roleFunctionality.Functionality = Functionality;
                    string[] roles = functionalityRole.Roles.Split(',');
                    foreach (var role in roles)
                    {
                        if(role == "Auctioneer")
                        {

                        }
                        else if (role != "")
                        {
                            Roles.Add(_mapper.Map<RoleResponse>(await _dataContext.Roles.Where(a => a.Name == role).FirstOrDefaultAsync()));
                        }
                        else if (role == "BudgetOfficer")
                        {
                            Roles.Add(_mapper.Map<RoleResponse>(await _dataContext.Roles.Where(a => a.Name == "Budget Officer").FirstOrDefaultAsync()));
                        }
                    }
                    roleFunctionality.Roles = Roles;
                    if (roleFunctionality.Roles.Count != 0)
                    {
                        roleFunctionalities.Add(roleFunctionality);
                    }
                }
                var result = await PostAssignRolesToFunctionality(roleFunctionalities);
             }
          
           // var result = await PostAssignRolesToFunctionality(roleFunctionalities);

           return StatusCode(StatusCodes.Status200OK);
        }

        [HttpGet("Users/Roles")]
        public async Task<ActionResult> GetUsersRoles()
        {
            var result = await _userManagementRepository.GetUsersRoles();

            if (result.StatusCode == Utils.Success)
            {
                result.ObjectValue = _mapper.Map<List<UserRoleResponse>>(((List<UserRole>)result.ObjectValue));
                return StatusCode(StatusCodes.Status200OK, result.ObjectValue);
            }
            else if (result.StatusCode == Utils.NotFound)
            {
                return StatusCode(StatusCodes.Status404NotFound, result);
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, result);
            }
        }

        [HttpPost("ProjectModules/Create")]
        public async Task<ActionResult> PostProjectModule(List<ProjectModuleRequest> projectModules)
        {
            var result = await _userManagementRepository.CreateProjectModule(_mapper.Map<List<ProjectModule>>(projectModules));

            if (result.StatusCode == Utils.Success)
            {
                result.ObjectValue = _mapper.Map<List<ProjectModuleResponse>>(((List<ProjectModule>)result.ObjectValue));
                return StatusCode(StatusCodes.Status200OK, result.ObjectValue);
            }
            else if (result.StatusCode == Utils.NotFound)
            {
                return StatusCode(StatusCodes.Status404NotFound, result);
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, result);
            }
        }

        [HttpGet("ProjectModules")]
        public async Task<ActionResult> GetProjectModules()
        {
            var result = await _userManagementRepository.GetProjectModules();

            if (result.StatusCode == Utils.Success)
            {
                result.ObjectValue = _mapper.Map<List<ProjectModuleResponse>>(((List<ProjectModule>)result.ObjectValue));
                return StatusCode(StatusCodes.Status200OK, result.ObjectValue);
            }
            else if (result.StatusCode == Utils.NotFound)
            {
                return StatusCode(StatusCodes.Status404NotFound, result);
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, result);
            }
        }

        [HttpPost("ProjectModules/Delete")]
        public async Task<ActionResult> DeleteProjectModule(List<ProjectModuleResponse> projectModules)
        {
            var result = await _userManagementRepository.DeleteProjectModule(projectModules);

            if (result.StatusCode == Utils.Success)
            {
                result.ObjectValue = _mapper.Map<List<ProjectModuleResponse>>(((List<ProjectModule>)result.ObjectValue));
                return StatusCode(StatusCodes.Status200OK, result.ObjectValue);
            }
            else if (result.StatusCode == Utils.NotFound)
            {
                return StatusCode(StatusCodes.Status404NotFound, result);
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, result);
            }
        }

        [HttpPost("Functionalities/Create")]
        public async Task<ActionResult> PostFunctionality(List<FunctionalityRequest> functionalities)
        {
            var result = await _userManagementRepository.CreateFunctionality(_mapper.Map<List<Functionality>>(functionalities));

            if (result.StatusCode == Utils.Success)
            {
                result.ObjectValue = _mapper.Map<List<FunctionalityResponse>>(((List<Functionality>)result.ObjectValue));
                return StatusCode(StatusCodes.Status200OK, result.ObjectValue);
            }
            else if (result.StatusCode == Utils.NotFound)
            {
                return StatusCode(StatusCodes.Status404NotFound, result);
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, result);
            }
        }

        [HttpGet("Functionalities")]
        public async Task<ActionResult> GetFunctionalities()
        {
            var result = await _userManagementRepository.GetFunctionalities();

            if (result.StatusCode == Utils.Success)
            {
                result.ObjectValue = _mapper.Map<List<FunctionalityResponse>>(((List<Functionality>)result.ObjectValue));
                return StatusCode(StatusCodes.Status200OK, result.ObjectValue);
            }
            else if (result.StatusCode == Utils.NotFound)
            {
                return StatusCode(StatusCodes.Status404NotFound, result);
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, result);
            }
        }

        [HttpPost("Functionalities/Delete")]
        public async Task<ActionResult> DeleteFunctionality(List<FunctionalityResponse> functionalities)
        {
            var result = await _userManagementRepository.DeleteFunctionality(functionalities);

            if (result.StatusCode == Utils.Success)
            {
                result.ObjectValue = _mapper.Map<List<FunctionalityResponse>>(((List<Functionality>)result.ObjectValue));
                return StatusCode(StatusCodes.Status200OK, result.ObjectValue);
            }
            else if (result.StatusCode == Utils.NotFound)
            {
                return StatusCode(StatusCodes.Status404NotFound, result);
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, result);
            }
        }

        [HttpPost("Functionalities/Roles/Assign")]
        public async Task<ActionResult> PostAssignRolesToFunctionality(List<RoleFunctionalityAssignmentRequest> roleFunctionalityAssignmentRequest)
        {
            var result = await _userManagementRepository.AssignRolesToFunctionality(roleFunctionalityAssignmentRequest);

            if (result.StatusCode == Utils.Success)
            {
                result.ObjectValue = _mapper.Map<List<FunctionalityRoleResponse>>(((List<FunctionalityRole>)result.ObjectValue));
                return StatusCode(StatusCodes.Status200OK, result.ObjectValue);
            }
            else if (result.StatusCode == Utils.NotFound)
            {
                return StatusCode(StatusCodes.Status404NotFound, result);
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, result);
            }
        }

        [HttpPost("Roles/Functionalities/Assign")]
        public async Task<ActionResult> PostRoleAndAssignFunctionalitiesToRole(List<FunctionalityRoleAssignmentRequest> functionalityRoleAssignmentRequests)
        {
            var result = await _userManagementRepository.CreateRoleAndAssignFunctionalitiesToRole(functionalityRoleAssignmentRequests);

            if (result.StatusCode == Utils.Success)
            {
                result.ObjectValue = _mapper.Map<List<FunctionalityRoleResponse>>(((List<FunctionalityRole>)result.ObjectValue));
                return StatusCode(StatusCodes.Status200OK, result.ObjectValue);
            }
            else if (result.StatusCode == Utils.NotFound)
            {
                return StatusCode(StatusCodes.Status404NotFound, result);
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, result);
            }
        }

        [HttpGet("Functionalities/Roles")]
        public async Task<ActionResult> GetFunctionalitiesRoles()
        {
            var result = await _userManagementRepository.GetFunctionalitiesRoles();

            if (result.StatusCode == Utils.Success)
            {
                result.ObjectValue = _mapper.Map<List<FunctionalityRoleResponse>>(((List<FunctionalityRole>)result.ObjectValue));
                return StatusCode(StatusCodes.Status200OK, result.ObjectValue);
            }
            else if (result.StatusCode == Utils.NotFound)
            {
                return StatusCode(StatusCodes.Status404NotFound, result);
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, result);
            }
        }
    }
}
*/