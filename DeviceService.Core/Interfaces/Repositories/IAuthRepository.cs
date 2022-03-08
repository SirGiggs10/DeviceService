using Microsoft.AspNetCore.Identity;
using Ayuda_Help_Desk.Data;
using Ayuda_Help_Desk.Dtos.General;
using Ayuda_Help_Desk.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ayuda_Help_Desk.Dtos;
using Ayuda_Help_Desk.DTOs.Auth;
using Ayuda_Help_Desk.Dtos.Auth;

namespace Ayuda_Help_Desk.Interfaces
{
    public interface IAuthRepository
    {
        public Task<ReturnResponse> LoginUser(UserForLoginDto userLoginDetails, string secretKey);    
        public Task<ReturnResponse> VerifyUserEmailAddress(UserEmailRequest userEmailRequest, string secretKey);
        public Task<ReturnResponse> VerifyUserMobileEmailAddress(UserEmailCode totp);
        public Task<ReturnResponse> ResetPasswordSendMail(ResetPasswordSendMailRequest resetPasswordRequest);
        public Task<ReturnResponse> ResetPasswordSendMailCode(ResetPasswordSendMailRequest resetPasswordRequest);
        public Task<ReturnResponse> ChangePassword(ChangePasswordRequest changePasswordRequest);
        public Task<ReturnResponse> ResetPasswordSetNewPassword(ResetPasswordRequest resetPasswordRequest);
        public Task<ReturnResponse> ResetPasswordCodeVerification(ResetPasswordCodeRequest resetPasswordRequest);
        public string GenerateJwtToken(User user, string secretKey);
        public Task<ReturnResponse> ResendUserEmailVerificationLink(EmailVerificationRequest emailVerificationRequest);
        public Task<ReturnResponse> ResendUserMobileEmailVerificationCode(EmailVerificationRequest emailVerificationRequest);
        public string GetUserEmailVerificationLink(string userToken);
        public Task<ReturnResponse> GetUser(int userId);
    }
}
