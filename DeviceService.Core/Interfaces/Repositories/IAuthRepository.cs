using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeviceService.Core.Dtos.Global;
using DeviceService.Core.Dtos.Auth;

namespace DeviceService.Core.Interfaces.Repositories
{
    public interface IAuthRepository
    {
        public Task<ReturnResponse> LoginUser(UserForLoginDto userLoginDetails);
        public Task<ReturnResponse> ChangePassword(ChangePasswordRequest changePasswordRequest);
    }
}
