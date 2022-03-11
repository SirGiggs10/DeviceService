using System.Net;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using DeviceService.Core.Interfaces.Repositories;
using DeviceService.Core.Data.DataContext;
using DeviceService.Core.Entities;
using DeviceService.Core.Dtos.Global;
using DeviceService.Core.Helpers.Common;
using DeviceService.Core.Dtos.Auth;
using DeviceService.Core.Helpers.Common.JWT;
using DeviceService.Core.Helpers.ConfigurationSettings.ConfigManager;

namespace DeviceService.Core.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly DeviceContext _dataContext;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IGlobalRepository _globalRepository;

        public AuthRepository(DeviceContext dataContext, UserManager<User> userManager, SignInManager<User> signInManager, IGlobalRepository globalRepository)
        {
            _dataContext = dataContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _globalRepository = globalRepository;
        }

        public async Task<ReturnResponse> LoginUser(UserForLoginDto userLoginDetails)
        {
            try
            {
                var user = await _userManager.Users.Where(a => a.NormalizedEmail == userLoginDetails.EmailAddress.ToUpper()).Include(b => b.UserRoles).ThenInclude(c => c.Role).FirstOrDefaultAsync();

                if (user == null)
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.SignInError,
                        StatusMessage = Utils.StatusMessageSignInError
                    };
                }

                var result = await _signInManager.CheckPasswordSignInAsync(user, userLoginDetails.Password, false);

                if (result.Succeeded)
                {
                    user.SecondToLastLoginDateTime = user.LastLoginDateTime;
                    user.LastLoginDateTime = DateTimeOffset.Now;

                    var updateResult = await _userManager.UpdateAsync(user);
                    if (!updateResult.Succeeded)
                    {
                        return new ReturnResponse()
                        {
                            StatusCode = Utils.NotSucceeded,
                            StatusMessage = Utils.StatusMessageNotSucceeded
                        };
                    }

                    //Build User Claims Object to use to Generate JWT
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Id.ToString()),
                        new Claim(Utils.ClaimType_UserEmail, user.UserName),
                        new Claim(ClaimTypes.NameIdentifier, user.UserTypeId.ToString()),
                        new Claim(Utils.ClaimType_UserType, user.UserType.ToString())
                    };

                    var roles = _userManager.GetRolesAsync(user).Result;

                    foreach (var role in roles)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role));
                    }

                    return new ReturnResponse()
                    {
                        StatusCode = Utils.Success,
                        StatusMessage = "Login Successful",
                        ObjectValue = new UserDetails
                        {
                            Token = JWTHelper.GenerateJwtToken(claims, ConfigSettings.AppSetting.Secret),
                            User = user
                        }
                    };
                }

                return new ReturnResponse()
                {
                    StatusCode = Utils.SignInError,
                    StatusMessage = Utils.StatusMessageSignInError
                };
            }
            catch(Exception ex)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.StatusCode_ExceptionError,
                    StatusMessage = Utils.StatusMessage_ExceptionError
                };
            }
        }

        public async Task<ReturnResponse> ChangePassword(ChangePasswordRequest changePasswordRequest)
        {
            try
            {
                if (changePasswordRequest == null || string.IsNullOrWhiteSpace(changePasswordRequest.OldPassword) || string.IsNullOrWhiteSpace(changePasswordRequest.NewPassword))
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.ObjectNull,
                        StatusMessage = Utils.StatusMessageObjectNull
                    };
                }

                if (changePasswordRequest.OldPassword == changePasswordRequest.NewPassword)
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.PreviousPasswordStorageError,
                        StatusMessage = Utils.StatusMessagePreviousPasswordStorageError
                    };
                }


                var loggedInUser = _globalRepository.GetUserInformation();
                var user = await _userManager.FindByIdAsync(loggedInUser.UserId);

                if (user != null)
                {
                    //HASH THE NEW PASSWORD
                    var newPasswordHash = _userManager.PasswordHasher.HashPassword(user, changePasswordRequest.NewPassword);

                    var result = await _userManager.ChangePasswordAsync(user, changePasswordRequest.OldPassword, changePasswordRequest.NewPassword);

                    if (!result.Succeeded)
                    {
                        return new ReturnResponse()
                        {
                            StatusCode = Utils.SaveError,
                            StatusMessage = Utils.StatusMessageSaveError
                        };
                    }

                    return new ReturnResponse()
                    {
                        StatusCode = Utils.Success,
                        StatusMessage = "Password Changed Successfully",
                        ObjectValue = user
                    };
                }

                return new ReturnResponse()
                {
                    StatusCode = Utils.NotFound,
                    StatusMessage = Utils.StatusMessageNotFound
                };
            }
            catch(Exception ex)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.StatusCode_ExceptionError,
                    StatusMessage = Utils.StatusMessage_ExceptionError
                };
            }
        }
    }
}
