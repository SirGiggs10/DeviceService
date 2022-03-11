using DeviceService.Core.Dtos.Global;
using DeviceService.Core.Dtos.User;
using DeviceService.Core.Helpers.Pagination;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DeviceService.Core.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<ReturnResponse<UserResponse>> CreateUser(UserRequest userRequest);
        Task<ReturnResponse<UserResponse>> GetUser(int userId);
        Task<ReturnResponse<List<UserResponse>>> GetUsers(UserParams userParam);
        Task<ReturnResponse<UserResponse>> UpdateUser(int userId, UserToUpdate userToUpdate);
        Task<ReturnResponse<UserResponse>> DeleteUser(int userId);
    }
}
