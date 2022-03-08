using Ayuda_Help_Desk.Data;
using Ayuda_Help_Desk.Dtos.General;
using Ayuda_Help_Desk.Interfaces;
using Ayuda_Help_Desk.Models;
using System.Net;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Ayuda_Help_Desk.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using Ayuda_Help_Desk.Dtos;
using Ayuda_Help_Desk.DTOs.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Ayuda_Help_Desk.Dtos.Auth;
using OtpNet;
using Microsoft.Extensions.Caching.Memory;

namespace Ayuda_Help_Desk.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly DataContext _dataContext;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IGlobalRepository _globalRepository;
        private readonly IConfiguration _configuration;
        private readonly IMailController _mailController;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private IMemoryCache _cache;

        public AuthRepository(DataContext dataContext, IMemoryCache cache, UserManager<User> userManager, IConfiguration configuration, IMailController mailController, SignInManager<User> signInManager, IGlobalRepository globalRepository, IHttpContextAccessor httpContextAccessor)
        {
            _dataContext = dataContext;
            _cache = cache;
            _userManager = userManager;
            _signInManager = signInManager;
            _globalRepository = globalRepository;
            _configuration = configuration;
            _mailController = mailController;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ReturnResponse> LoginUser(UserForLoginDto userLoginDetails, string secretKey)
        {
            var user = await _userManager.Users.Where(a => a.NormalizedEmail == userLoginDetails.EmailAddress.ToUpper())
                .Include(b => b.UserRoles).ThenInclude(c => c.Role).FirstOrDefaultAsync();

            if (user == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.SignInError,
                    StatusMessage = "Incorrect Email Address or Password!!!"
                };
            }
            var result = await _signInManager.CheckPasswordSignInAsync(user, userLoginDetails.Password, false);
            if (result.Succeeded)
            {
                object userInfo = null;
                if (user.UserType == Utils.Customer)
                {
                    userInfo = await _globalRepository.Get<Customer>(user.UserTypeId);
                }
                else if (user.UserType == Utils.Staff)
                {
                    var staffDetail = await _dataContext.Staff.Include(d => d.Department).Include(e => e.SubUnit).Include(b => b.Branch).Where(s => s.StaffId == user.UserTypeId).FirstOrDefaultAsync();
                    staffDetail.Supervisor = await _dataContext.Users.Where(u => u.UserTypeId == staffDetail.SupervisorId).Include(s => s.Staff).FirstOrDefaultAsync();
                    userInfo = staffDetail;
                }
                else
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.BadRequest,
                        StatusMessage = "User Type does not exist!!!"
                    };
                }

                user.SecondToLastLoginDateTime = user.LastLoginDateTime;
                user.LastLoginDateTime = DateTimeOffset.Now;
                var updateResult = await _userManager.UpdateAsync(user);
                if(!updateResult.Succeeded)
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.NotSucceeded,
                        StatusMessage = Utils.StatusMessageNotSucceeded
                    };
                }

                return new ReturnResponse()
                {
                    StatusCode = Utils.Success,
                    ObjectValue = new UserDetails
                    {
                        Token = GenerateJwtToken(user, secretKey),
                        User = user,
                        userProfile = userInfo
                    }
                };
            }

            return new ReturnResponse()
            {
                StatusCode = Utils.SignInError,
                StatusMessage = Utils.StatusMessageSignInError
            };
        }
     
        public async Task<ReturnResponse> VerifyUserEmailAddress(UserEmailRequest userEmailRequest, string secretKey)
        {
            try
            {
                //FIRST OF ALL CONFIRM EMAIL TOKEN BEFORE USING IT TO GET USER DETAILS
                //Continue
                bool emailTokenConfirmed;
                //NO LOGIN REQUIRED TO CONFIRM EMAIL SO...
                //string userEmail = Encoding.UTF8.GetString(Convert.FromBase64String(userEmailRequest.EmailConfirmationLinkToken.Replace("ngiSlauqe", "=")));
                userEmailRequest.EmailConfirmationLinkToken = userEmailRequest.EmailConfirmationLinkToken.Replace('-', '%');
                var originalUserToken = Uri.UnescapeDataString(userEmailRequest.EmailConfirmationLinkToken);
                string[] emailTokenVal = originalUserToken.Split('#', 2);
                string userEmailTokenBase64 = "";
                string userEmailBase64 = "";
                if (emailTokenVal.Length == 1)
                {
                    userEmailTokenBase64 = emailTokenVal[0];
                }
                else
                {
                    userEmailTokenBase64 = emailTokenVal[0];
                    userEmailBase64 = emailTokenVal[1];
                }

                string userEmail = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(userEmailBase64));
                var user = await _userManager.FindByEmailAsync(userEmail);

                if (user != null)
                {
                    if (user.EmailConfirmed)
                    {
                        //EMAIL ALREADY CONFIRMED
                        return new ReturnResponse()
                        {
                            StatusCode = Utils.EmailAlreadyConfirmed,
                            StatusMessage = Utils.StatusMessageEmailAlreadyConfirmed
                        };
                    }
                    else
                    {
                        IdentityResult identityResult = await _userManager.ConfirmEmailAsync(user, userEmailTokenBase64);
                        if (identityResult.Succeeded)
                        {
                            emailTokenConfirmed = true;
                        }
                        else
                        {
                            emailTokenConfirmed = false;
                        }

                        if (emailTokenConfirmed)
                        {
                            //AFTER EMAIL CONFIRMATION AUTOMATICALLY LOG THE USER IN
                            //var appUser = await _userManager.Users.FirstOrDefaultAsync(c => c.NormalizedEmail == user.Email.ToUpper());
                            if(user.UserType == Utils.Customer)
                            {
                                //CUSTOMER
                                //AFTER EMAIL CONFIRMATION AUTOMATICALLY LOG THE USER IN
                                var loginResult = await LogUserInWithoutPassword(user);
                                if (loginResult.StatusCode == Utils.Success)
                                {
                                    return loginResult;
                                }
                                else
                                {
                                    return loginResult;
                                }
                            }
                            else if(user.UserType == Utils.Staff)
                            {
                                //STAFF
                                return new ReturnResponse()
                                {
                                    StatusCode = Utils.Success,
                                    ObjectValue = new UserDetails
                                    {
                                        User = user,
                                        userProfile = await _globalRepository.Get<Staff>(user.UserTypeId)
                                    }
                                };
                            }
                            else
                            {
                                //INVALID USERTYPE
                                return new ReturnResponse()
                                {
                                    StatusCode = Utils.InvalidUserType,
                                    StatusMessage = Utils.StatusMessageInvalidUserType
                                };
                            }
                        }
                        else
                        {
                            return new ReturnResponse()
                            {
                                StatusCode = Utils.NotSucceeded,
                                StatusMessage = Utils.StatusMessageNotSucceeded
                            };
                        }
                    }
                }
                else
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.NotFound,
                        StatusMessage = Utils.StatusMessageNotFound
                    };
                }
            }
            catch (NullReferenceException)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotSucceeded,
                    StatusMessage = Utils.StatusMessageNotSucceeded
                };
            }
        }

        public async Task<ReturnResponse> VerifyUserMobileEmailAddress(UserEmailCode otp)
        {
            try
            {
                //FIRST OF ALL CONFIRM EMAIL TOKEN BEFORE USING IT TO GET USER DETAILS
                //Continue
                bool emailTokenConfirmed;
                //NO LOGIN REQUIRED TO CONFIRM EMAIL SO...'
                string code = string.Empty;
                _cache.TryGetValue(otp.EmailAddress, out code);

                if(otp.Otp != code)
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.BadRequest,
                        StatusMessage = "OTP is incorrect!!!"
                    };
                }
               
                var user = await _userManager.FindByEmailAsync(otp.EmailAddress);
                var userTokenVal = await _userManager.GenerateEmailConfirmationTokenAsync(user);


                if (user != null)
                {
                    if (user.EmailConfirmed)
                    {
                        //EMAIL ALREADY CONFIRMED
                        return new ReturnResponse()
                        {
                            StatusCode = Utils.EmailAlreadyConfirmed,
                            StatusMessage = Utils.StatusMessageEmailAlreadyConfirmed
                        };
                    }
                    else
                    {
                        IdentityResult identityResult = await _userManager.ConfirmEmailAsync(user, userTokenVal);
                        if (identityResult.Succeeded)
                        {
                            emailTokenConfirmed = true;
                        }
                        else
                        {
                            emailTokenConfirmed = false;
                        }

                        if (emailTokenConfirmed)
                        {
                            //AFTER EMAIL CONFIRMATION AUTOMATICALLY LOG THE USER IN
                            //var appUser = await _userManager.Users.FirstOrDefaultAsync(c => c.NormalizedEmail == user.Email.ToUpper());
                            if (user.UserType == Utils.Customer)
                            {
                                //CUSTOMER
                                _cache.Remove(otp.Otp);
                                //AFTER EMAIL CONFIRMATION AUTOMATICALLY LOG THE USER IN
                                var loginResult = await LogUserInWithoutPassword(user);
                                if (loginResult.StatusCode == Utils.Success)
                                {
                                    return loginResult;
                                }
                                else
                                {
                                    return loginResult;
                                }
                            }
                            else if (user.UserType == Utils.Staff)
                            {
                                //STAFF
                                return new ReturnResponse()
                                {
                                    StatusCode = Utils.Success,
                                    ObjectValue = new UserDetails
                                    {
                                        User = user,
                                        userProfile = await _globalRepository.Get<Staff>(user.UserTypeId)
                                    }
                                };
                            }
                            else
                            {
                                //INVALID USERTYPE
                                return new ReturnResponse()
                                {
                                    StatusCode = Utils.InvalidUserType,
                                    StatusMessage = Utils.StatusMessageInvalidUserType
                                };
                            }
                        }
                        else
                        {
                            return new ReturnResponse()
                            {
                                StatusCode = Utils.NotSucceeded,
                                StatusMessage = Utils.StatusMessageNotSucceeded
                            };
                        }
                    }
                }
                else
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.NotFound,
                        StatusMessage = Utils.StatusMessageNotFound
                    };
                }
            }
            catch (NullReferenceException)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotSucceeded,
                    StatusMessage = Utils.StatusMessageNotSucceeded
                };
            }
        }

        public async Task<ReturnResponse> ResetPasswordCodeVerification(ResetPasswordCodeRequest resetPasswordRequest)
        {
            //USER SUBMITS THE NEW PASSWORD BEFORE PASSWORD RESET TOKEN IS CONFIRMED
            if ((resetPasswordRequest == null) || string.IsNullOrWhiteSpace(resetPasswordRequest.NewPassword))
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.ObjectNull,
                    StatusMessage = "Please Input a New Password!!!"
                };

            }

            try
            {
                string code = string.Empty;
                _cache.TryGetValue(resetPasswordRequest.EmailAddress, out code);

                if (resetPasswordRequest.Otp != code)
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.BadRequest,
                        StatusMessage = "OTP is incorrect!!!"
                    };
                }

                var user = await _userManager.FindByEmailAsync(resetPasswordRequest.EmailAddress);

                if (user != null)
                {
                    //HASH THE NEW PASSWORD
                    var newPasswordHash = _userManager.PasswordHasher.HashPassword(user, resetPasswordRequest.NewPassword);

                    //CHECK USERPREVIOUSPASSWORD TABLE TO MAKE SURE NEWPASSWORD HAS NOT BEEN USED BEFORE
                    var allPreviousPasswordsForUser = await _dataContext.UserPreviousPassword.Where(a => a.UserId == user.Id).Select(b => b.HashedPreviousPassword).ToListAsync();
                    if (allPreviousPasswordsForUser.Any(a => a == newPasswordHash))
                    {
                        //NEW PASSWORD HAS BEEN USED BEFORE
                        return new ReturnResponse()
                        {
                            StatusCode = Utils.NewPasswordError,
                            StatusMessage = "Please use a New Password that has not been used!!!"
                        };
                    }

                    //CHECK USER TABLE TO MAKE SURE PASSWORD HAS NOT BEEN USED BEFORE
                    if (user.PasswordHash == newPasswordHash)
                    {
                        //NEW PASSWORD EQUALS CURRENT USER PASSWORD
                        return new ReturnResponse()
                        {
                            StatusCode = Utils.NewPasswordError,
                            StatusMessage = Utils.StatusMessageNewPasswordError
                        };
                    }

                    var result = await StoreUserPreviousPassword(user);
                    if (!result)
                    {
                        return new ReturnResponse()
                        {
                            StatusCode = Utils.PreviousPasswordStorageError,
                            StatusMessage = Utils.StatusMessagePreviousPasswordStorageError
                        };
                    }
                    var userTokenVal = await _userManager.GeneratePasswordResetTokenAsync(user);
                    await _dataContext.SaveChangesAsync();
                    bool passwordTokenConfirmed;
                  

                    IdentityResult identityResult = await _userManager.ResetPasswordAsync(user, userTokenVal, resetPasswordRequest.NewPassword);
                    if (identityResult.Succeeded)
                    {
                        passwordTokenConfirmed = true;
                    }
                    else
                    {
                        passwordTokenConfirmed = false;
                    }

                    if (passwordTokenConfirmed)
                    {
                        //AFTER PASSWORD TOKEN CONFIRMATION...LOG USER IN
                        var loginResult = await LoginUser(new UserForLoginDto()
                        {
                            EmailAddress = user.Email,
                            Password = resetPasswordRequest.NewPassword
                        }, _configuration.GetValue<string>("AppSettings:Secret"));

                        if (loginResult.StatusCode == Utils.Success)
                        {
                            //DO NOTHING
                        }
                        else
                        {
                            //DO NOTHING
                        }

                        return loginResult;
                    }
                    else
                    {
                        return new ReturnResponse()
                        {
                            StatusCode = Utils.NotSucceeded,
                            StatusMessage = Utils.StatusMessageNotSucceeded
                        };
                    }
                }
                else
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.NotFound,
                        StatusMessage = "Incorrect Token!!!"
                    };
                }
            }
            catch (NullReferenceException)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotSucceeded,
                    StatusMessage = Utils.StatusMessageNotSucceeded
                };
            }
        }


        public async Task<ReturnResponse> ResetPasswordSendMailCode(ResetPasswordSendMailRequest resetPasswordRequest)
        {
            if ((resetPasswordRequest == null) || string.IsNullOrWhiteSpace(resetPasswordRequest.EmailAddress))
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.ObjectNull,
                    StatusMessage = Utils.StatusMessageObjectNull
                };
            }

            var userDetails = await _userManager.FindByEmailAsync(resetPasswordRequest.EmailAddress);
            if (userDetails == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotFound,
                    StatusMessage = Utils.StatusMessageNotFound
                };
            }
            var userTokenVal = await _userManager.GeneratePasswordResetTokenAsync(userDetails);
            await _dataContext.SaveChangesAsync();
            var key = _configuration.GetValue<string>("AppSettings:Secret");
            // Creating byte array of string length 
            byte[] secretKey = new byte[key.Length];

            // converting each character into byte 
            // and store it
            for (int i = 0; i < key.Length; i++)
            {
                secretKey[i] = Convert.ToByte(key[i]);
            }
            var totp = new Totp(secretKey);

            var otpCode = totp.ComputeTotp();
            var emailMessage2 = otpCode;

            var emailSubject = "PASSWORD RESET";
            var currentUserName = "";
            if (userDetails.UserType == Utils.Customer)
            {
                currentUserName = (await _dataContext.Customer.Where(c => c.CustomerId == userDetails.UserTypeId).FirstOrDefaultAsync()).FullName;
            }
            else if (userDetails.UserType == Utils.Staff)
            {
                currentUserName = (await _dataContext.Staff.Where(c => c.StaffId == userDetails.UserTypeId).FirstOrDefaultAsync()).FullName;
            }
            else
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.BadRequest,
                    StatusMessage = Utils.StatusMessageBadRequest
                };
            }

            var userName = currentUserName.Split();
            var emailMessage1 = "Please use the code below to complete your Password Reset.";

            string emailBody = _globalRepository.GetMailBodyTemplate(userName[0], userName[1], "", emailMessage1, emailMessage2, "resetpasswordcode.html");


            //SEND MAIL TO CUSTOMER TO RESET PASSWORD
            MailModel mailObj = new MailModel(_configuration.GetValue<string>("AyudaEmailAddress"), _configuration.GetValue<string>("AyudaEmailName"), resetPasswordRequest.EmailAddress, emailSubject, emailBody);
            var response = await _mailController.SendMail(mailObj);
            if (response.StatusCode.Equals(HttpStatusCode.Accepted))
            {
                var cacheEntry = otpCode;
                var cacheExpiryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpiration = DateTime.Now.AddMinutes(30),
                    Priority = CacheItemPriority.High,
                    SlidingExpiration = TimeSpan.FromMinutes(10),
                    Size = 1024,
                };
                _cache.Set(resetPasswordRequest.EmailAddress, otpCode, cacheExpiryOptions);
                return new ReturnResponse()
                {
                    StatusMessage = "Mail Sent Successfully",
                    StatusCode = Utils.Success,
                    ObjectValue = userDetails
                };
            }
            else
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.MailFailure,
                    StatusMessage = Utils.StatusMessageMailFailure
                };
            }
        }


        public async Task<ReturnResponse> ResetPasswordSendMail(ResetPasswordSendMailRequest resetPasswordRequest)
        {
            if ((resetPasswordRequest == null) || string.IsNullOrWhiteSpace(resetPasswordRequest.EmailAddress))
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.ObjectNull,
                    StatusMessage = Utils.StatusMessageObjectNull
                };
            }

            var userDetails = await _userManager.FindByEmailAsync(resetPasswordRequest.EmailAddress);
            if (userDetails == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotFound,
                    StatusMessage = "A User with this Email Address does not exist!!!"
                };
            }
            var userTokenVal = await _userManager.GeneratePasswordResetTokenAsync(userDetails);
            await _dataContext.SaveChangesAsync();
            string hashedEmail = GetHashedEmail(userDetails.Email);
            /*var shortToken = userTokenVal.Substring(0, 7);
            userDetails.ShortToken = shortToken;
            userDetails.LongToken = userTokenVal;
            var saveThis = _globalRepository.SaveAll();*/
            string fullToken = userTokenVal + "#" + hashedEmail;
            var passwordResetLink = GetResetPasswordLink(fullToken);
            if (passwordResetLink == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.ObjectNull,
                    StatusMessage = Utils.StatusMessageObjectNull
                };
            }

            var emailSubject = "PASSWORD RESET";
            var currentUserName = "";
            if (userDetails.UserType == Utils.Customer)
            {
                currentUserName = (await _dataContext.Customer.Where(c => c.CustomerId == userDetails.UserTypeId).FirstOrDefaultAsync()).FullName;
            }
            else if (userDetails.UserType == Utils.Staff)
            {
                currentUserName = (await _dataContext.Staff.Where(c => c.StaffId == userDetails.UserTypeId).FirstOrDefaultAsync()).FullName;
            }
            else
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.BadRequest,
                    StatusMessage = Utils.StatusMessageBadRequest
                };
            }

            var userName = currentUserName.Split();          
            string link = passwordResetLink;
            var emailMessage1 = "";
            var emailMessage2 = "Please click the button below to complete your Password Reset.";

            string emailBody = _globalRepository.GetMailBodyTemplate(userName[0], userName[1], link, emailMessage1, emailMessage2, "resetpassword.html");


            //SEND MAIL TO CUSTOMER TO RESET PASSWORD
            MailModel mailObj = new MailModel(_configuration.GetValue<string>("AyudaEmailAddress"), _configuration.GetValue<string>("AyudaEmailName"), resetPasswordRequest.EmailAddress, emailSubject, emailBody);
            var response = await _mailController.SendMail(mailObj);
            if(response.StatusCode.Equals(HttpStatusCode.Accepted))
            {
                return new ReturnResponse()
                {
                    StatusMessage = "Mail Sent Successfully",
                    StatusCode = Utils.Success,
                    ObjectValue = userDetails
                };
            }
            else
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.MailFailure,
                    StatusMessage = Utils.StatusMessageMailFailure
                };
            }
        }

        public async Task<ReturnResponse> ResetPasswordSetNewPassword(ResetPasswordRequest resetPasswordRequest)
        {
            //USER SUBMITS THE NEW PASSWORD BEFORE PASSWORD RESET TOKEN IS CONFIRMED
            if ((resetPasswordRequest == null) || string.IsNullOrWhiteSpace(resetPasswordRequest.NewPassword))
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.ObjectNull,
                    StatusMessage = "Please Input a New Password!!!"
                };
                
            }

            try
            {
                //FIRST OF ALL CONFIRM EMAIL TOKEN BEFORE USING IT TO SET USER PASSWORD
                bool passwordTokenConfirmed;
                //NO LOGIN REQUIRED TO CONFIRM EMAIL AND UPLOAD VENDOR DOCUMENTS SO...
                resetPasswordRequest.PasswordResetLinkToken = resetPasswordRequest.PasswordResetLinkToken.Replace('-', '%');
                var originalUserToken = Uri.UnescapeDataString(resetPasswordRequest.PasswordResetLinkToken);
                string[] emailTokenVal = originalUserToken.Split('#', 2);
                string userEmailTokenBase64 = "";
                string userEmailBase64 = "";
                if (emailTokenVal.Length == 1)
                {
                    userEmailTokenBase64 = emailTokenVal[0];
                }
                else
                {
                    userEmailTokenBase64 = emailTokenVal[0];
                    userEmailBase64 = emailTokenVal[1];
                }

                string userEmail = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(userEmailBase64));
                //string userEmail = Encoding.UTF8.GetString(Convert.FromBase64String(resetPasswordRequest.PasswordResetLinkToken.Replace("ngiSlauqe", "=")));
                var user = await _userManager.FindByEmailAsync(userEmail);

                if (user != null)
                {
                    //HASH THE NEW PASSWORD
                    var newPasswordHash = _userManager.PasswordHasher.HashPassword(user, resetPasswordRequest.NewPassword);

                    //CHECK USERPREVIOUSPASSWORD TABLE TO MAKE SURE NEWPASSWORD HAS NOT BEEN USED BEFORE
                    var allPreviousPasswordsForUser = await _dataContext.UserPreviousPassword.Where(a => a.UserId == user.Id).Select(b => b.HashedPreviousPassword).ToListAsync();
                    if(allPreviousPasswordsForUser.Any(a => a == newPasswordHash))
                    {
                        //NEW PASSWORD HAS BEEN USED BEFORE
                        return new ReturnResponse()
                        {
                            StatusCode = Utils.NewPasswordError,
                            StatusMessage = "Please use a New Password that has not been used!!!"
                        };
                    }

                    //CHECK USER TABLE TO MAKE SURE PASSWORD HAS NOT BEEN USED BEFORE
                    if(user.PasswordHash == newPasswordHash)
                    {
                        //NEW PASSWORD EQUALS CURRENT USER PASSWORD
                        return new ReturnResponse()
                        {
                            StatusCode = Utils.NewPasswordError,
                            StatusMessage = Utils.StatusMessageNewPasswordError
                        };
                    }

                    var result = await StoreUserPreviousPassword(user);
                    if(!result)
                    {
                        return new ReturnResponse()
                        {
                            StatusCode = Utils.PreviousPasswordStorageError,
                            StatusMessage = Utils.StatusMessagePreviousPasswordStorageError
                        };
                    }

                    IdentityResult identityResult = await _userManager.ResetPasswordAsync(user, userEmailTokenBase64, resetPasswordRequest.NewPassword);
                    if (identityResult.Succeeded)
                    {
                        passwordTokenConfirmed = true;
                    }
                    else
                    {
                        passwordTokenConfirmed = false;
                    }

                    if (passwordTokenConfirmed)
                    {
                        //AFTER PASSWORD TOKEN CONFIRMATION...LOG USER IN
                        var loginResult = await LoginUser(new UserForLoginDto()
                        {
                            EmailAddress = userEmail,
                            Password = resetPasswordRequest.NewPassword
                        }, _configuration.GetValue<string>("AppSettings:Secret"));

                        if(loginResult.StatusCode == Utils.Success)
                        {
                            //DO NOTHING
                        }
                        else
                        {
                            //DO NOTHING
                        }

                        return loginResult;
                    }
                    else
                    {
                        return new ReturnResponse()
                        {
                            StatusCode = Utils.NotSucceeded,
                            StatusMessage = Utils.StatusMessageNotSucceeded
                        };                        
                    }
                }
                else
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.NotFound,
                        StatusMessage = "Incorrect Token!!!"
                    };
                }
            }
            catch (NullReferenceException)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotSucceeded,
                    StatusMessage = Utils.StatusMessageNotSucceeded
                };
            }
        }

        public string GenerateJwtToken(User user, string secretKey)
        {
            //Get Customer info
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

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        private string GetHashedEmail(string emailVal)
        {
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(emailVal));//.Replace("=","ngiSlauqe");
        }

        private string GetResetPasswordLink(string resetPasswordToken)
        {
            var originUrls = new StringValues();           
            //CHECK LATER TO SEE IF ANY ORIGIN HEADER WILL BE SENT WITH THE REQUEST IF THE FRONTEND AND BACKEND ARE IN THE SAME DOMAIN...THAT IS IF THERE IS NO CORS
            var originHeadersGotten = _httpContextAccessor.HttpContext.Request.Headers.TryGetValue("Origin", out originUrls);
            if(originHeadersGotten)
            {
                var originUrl = originUrls.FirstOrDefault();
                if(string.IsNullOrWhiteSpace(originUrl))
                {
                    return null;
                }

                var convertedToken = Uri.EscapeDataString(resetPasswordToken);
                convertedToken = convertedToken.Replace('%', '-');
                string emailVerificationLink = originUrl + "/" + _configuration.GetValue<string>("UserPasswordResetLink") + "/" + convertedToken;
                return emailVerificationLink;
            }

            return null;
        }

        private string GetResendUserEmailVerificationLink(string userToken)
        {
            var originUrls = new StringValues();
            //CHECK LATER TO SEE IF ANY ORIGIN HEADER WILL BE SENT WITH THE REQUEST IF THE FRONTEND AND BACKEND ARE IN THE SAME DOMAIN...THAT IS IF THERE IS NO CORS
            var originHeadersGotten = _httpContextAccessor.HttpContext.Request.Headers.TryGetValue("Origin", out originUrls);
            if (originHeadersGotten)
            {
                var originUrl = originUrls.FirstOrDefault();
                if (string.IsNullOrWhiteSpace(originUrl))
                {
                    return null;
                }

                var convertedToken = Uri.EscapeDataString(userToken);
                convertedToken = convertedToken.Replace('%', '-');
                string emailVerificationLink = originUrl + "/" + _configuration.GetValue<string>("UserEmailConfirmationLink") + "/" + convertedToken;
                return emailVerificationLink;
            }

            return null;
        }

        private async Task<bool> StoreUserPreviousPassword(User user)
        {
            var userPreviousPassword = new UserPreviousPassword()
            {
                UserId = user.Id,
                HashedPreviousPassword = user.PasswordHash,
                CreatedAt = DateTimeOffset.Now
            };
            _globalRepository.Add(userPreviousPassword);
            var result = await _globalRepository.SaveAll();

            if(result != null)
            {
                if(!result.Value)
                {
                    return false;
                }

                return true;
            }

            return false;
        }

        public async Task<ReturnResponse> ResendUserEmailVerificationLink(EmailVerificationRequest emailVerificationRequest)
        {
            if (emailVerificationRequest == null || string.IsNullOrWhiteSpace(emailVerificationRequest.EmailAddress))
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.ObjectNull
                };
            }

            var user = await _userManager.FindByEmailAsync(emailVerificationRequest.EmailAddress);
            if(user != null)
            {
                //SEND MAIL TO USER TO CONFIRM EMAIL
                var userTokenVal = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                string hashedEmail = GetHashedEmail(user.Email);
                /*var shortToken = userTokenVal.Substring(0, 7);
                user.ShortToken = shortToken;
                user.LongToken = userTokenVal;
                var saveThis = _globalRepository.SaveAll();*/
                string fullToken = userTokenVal + "#" + hashedEmail;
                var emailVerificationLink = GetResendUserEmailVerificationLink(fullToken);
                if (emailVerificationLink == null)
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.ObjectNull,
                        StatusMessage = Utils.StatusMessageObjectNull
                    };
                }

                var emailSubject = "CONFIRM YOUR EMAIL ADDRESS";


                var currentUserName = "";

                if (user.UserType == Utils.Customer)    
                {
                    currentUserName = (await _dataContext.Customer.Where(c => c.CustomerId == user.UserTypeId).FirstOrDefaultAsync()).FullName;
                }
                else if (user.UserType == Utils.Staff)
                {
                    currentUserName = (await _dataContext.Staff.Where(c => c.StaffId == user.UserTypeId).FirstOrDefaultAsync()).FullName;
                }

                var userName = currentUserName.Split();

                string link = emailVerificationLink;
                var emailMessage1 = "";
                var emailMessage2 = "Please click the button below to complete your email verification and activate you account.";

                string emailBody = _globalRepository.GetMailBodyTemplate(userName[0], userName[1], link, emailMessage1, emailMessage2, "activation.html");

                //SEND MAIL TO CUSTOMER TO VERIFY EMAIL
                MailModel mailObj = new MailModel(_configuration.GetValue<string>("AyudaEmailAddress"), _configuration.GetValue<string>("AyudaEmailName"), user.Email, emailSubject, emailBody);
                var response = await _mailController.SendMail(mailObj);
                if (response.StatusCode.Equals(HttpStatusCode.Accepted))
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.Success,
                        StatusMessage = "Email Verification Link Sent Successfully!!!",
                        ObjectValue = user
                    };
                }
                else
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.MailFailure
                    };
                }
            }

            return new ReturnResponse()
            {
                StatusCode = Utils.NotFound
            };
        }

        public async Task<ReturnResponse> LogUserInWithoutPassword(User user)
        {
            object userInfo = null;
            if (user.UserType == Utils.Customer)
            {
                //CUSTOMER
                userInfo = await _globalRepository.Get<Customer>(user.UserTypeId);
            }
            else
            {
                //STAFF
                userInfo = await _globalRepository.Get<Staff>(user.UserTypeId);
            }
            
            if (userInfo == null)
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
                ObjectValue = new UserDetails
                {
                    Token = GenerateJwtToken(user, _configuration.GetValue<string>("AppSettings:Secret")),
                    User = user,
                    userProfile = userInfo
                }
            };
        }

        public string GetUserEmailVerificationLink(string userToken)
         {
            var originUrls = new StringValues();
            //CHECK LATER TO SEE IF ANY ORIGIN HEADER WILL BE SENT WITH THE REQUEST IF THE FRONTEND AND BACKEND ARE IN THE SAME DOMAIN...THAT IS IF THERE IS NO CORS
            var originHeadersGotten = _httpContextAccessor.HttpContext.Request.Headers.TryGetValue("Origin", out originUrls);
            if (originHeadersGotten)
            {
                var originUrl = originUrls.FirstOrDefault();
                if (string.IsNullOrWhiteSpace(originUrl))
                {
                    return null;
                }

                var convertedUserToken = Uri.EscapeDataString(userToken);
                convertedUserToken = convertedUserToken.Replace('%', '-');
                string emailVerificationLink = originUrl + "/" + _configuration.GetValue<string>("UserEmailConfirmationLink") + "/" + convertedUserToken;
                return emailVerificationLink;
            }

            return null;
        }

        public async Task<ReturnResponse> ChangePassword(ChangePasswordRequest changePasswordRequest)
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

                //CHECK USERPREVIOUSPASSWORD TABLE TO MAKE SURE NEWPASSWORD HAS NOT BEEN USED BEFORE
                var allPreviousPasswordsForUser = await _dataContext.UserPreviousPassword.Where(a => a.UserId == user.Id).Select(b => b.HashedPreviousPassword).ToListAsync();
                if (allPreviousPasswordsForUser.Any(a => a == newPasswordHash))
                {
                    //NEW PASSWORD HAS BEEN USED BEFORE
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.NewPasswordError,
                        StatusMessage = Utils.StatusMessageNewPasswordError
                    };
                }

                //CHECK USER TABLE TO MAKE SURE PASSWORD HAS NOT BEEN USED BEFORE
                if (user.PasswordHash.Equals(newPasswordHash))
                {
                    //NEW PASSWORD EQUALS CURRENT USER PASSWORD
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.NewPasswordError,
                        StatusMessage = Utils.StatusMessageNewPasswordError
                    };
                }

                var result1 = await StoreUserPreviousPassword(user);
                if (!result1)
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.PreviousPasswordStorageError,
                        StatusMessage = Utils.StatusMessagePreviousPasswordStorageError
                    };
                }
                var result = await _userManager.ChangePasswordAsync(user, changePasswordRequest.OldPassword, changePasswordRequest.NewPassword);

                if (result.Succeeded)
                {
                    var currentUserName = "";

                    if (user.UserType == Utils.Customer)
                    {
                        currentUserName = (await _dataContext.Customer.Where(c => c.CustomerId == user.UserTypeId).FirstOrDefaultAsync()).FullName;
                    }
                    else if (user.UserType == Utils.Staff)
                    {
                        currentUserName = (await _dataContext.Staff.Where(c => c.StaffId == user.UserTypeId).FirstOrDefaultAsync()).FullName;
                    }

                    var userName = currentUserName.Split();

                    string link = "";
                    var emailMessage1 = "";
                    var emailMessage2 = "Your Password has been changed successfully";

                    string emailBody = _globalRepository.GetMailBodyTemplate(userName[0], userName[1], link, emailMessage1, emailMessage2, "index.html");
                    var emailSubject = "PASSWORD CHANGED SUCCESSFULLY";
                    //SEND MAIL TO CUSTOMER TO VERIFY EMAIL
                    MailModel mailObj = new MailModel(_configuration.GetValue<string>("AyudaEmailAddress"), _configuration.GetValue<string>("AyudaEmailName"), user.Email, emailSubject, emailBody);
                    var response = await _mailController.SendMail(mailObj);
                    if (response.StatusCode.Equals(HttpStatusCode.Accepted))
                    {
                        return new ReturnResponse()
                        {
                            StatusCode = Utils.Success,
                            StatusMessage = "Password Changed Successfully",
                            ObjectValue = user
                        };
                    }
                    else
                    {
                        return new ReturnResponse()
                        {
                            StatusCode = Utils.MailFailure,
                            StatusMessage = Utils.StatusMessageMailFailure
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
            else
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotFound,
                    StatusMessage = Utils.StatusMessageNotFound
                };
            }

        }

        public async Task<ReturnResponse> GetUser(int userId)
        {
            var user = _userManager.Users.Where(a => a.Id == userId);
            var userDetails = user.FirstOrDefault();
            if (userDetails == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotFound,
                    StatusMessage = Utils.StatusMessageNotFound
                };
            }

            var userFullDetails = new User();
            if(userDetails.UserType == Utils.Staff)
            {
                userFullDetails = await user.Include(c => c.UserRoles).ThenInclude(d => d.Role).ThenInclude(j => j.SupportLevel).Include(b => b.Staff).ThenInclude(e => e.Department).Include(b => b.Staff).ThenInclude(f => f.SubUnit).Include(b => b.Staff).ThenInclude(g => g.Branch).FirstOrDefaultAsync();
            }
            else if(userDetails.UserType == Utils.Customer)
            {
                userFullDetails = await user.Include(c => c.UserRoles).ThenInclude(d => d.Role).Include(e => e.Customer).ThenInclude(f => f.CustomerType).FirstOrDefaultAsync();
            }
            else
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.InvalidUserType,
                    StatusMessage = Utils.StatusMessageInvalidUserType
                };
            }

            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                StatusMessage = Utils.StatusMessageSuccess,
                ObjectValue = userFullDetails
            };
        }

        public async Task<ReturnResponse> ResendUserMobileEmailVerificationCode(EmailVerificationRequest emailVerificationRequest)
        {
            if (emailVerificationRequest == null || string.IsNullOrWhiteSpace(emailVerificationRequest.EmailAddress))
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.ObjectNull
                };
            }

            var user = await _userManager.FindByEmailAsync(emailVerificationRequest.EmailAddress);
            if (user != null)
            {
                var key = _configuration.GetValue<string>("AppSettings:Secret");
                // Creating byte array of string length 
                byte[] secretKey = new byte[key.Length];

                // converting each character into byte 
                // and store it
                for (int i = 0; i < key.Length; i++)
                {
                    secretKey[i] = Convert.ToByte(key[i]);
                }

                var totp = new Totp(secretKey);

                var emailSubject = "CONFIRM YOUR EMAIL ADDRESS";


                var currentUserName = "";

                if (user.UserType == Utils.Customer)
                {
                    currentUserName = (await _dataContext.Customer.Where(c => c.CustomerId == user.UserTypeId).FirstOrDefaultAsync()).FullName;
                }
                else if (user.UserType == Utils.Staff)
                {
                    currentUserName = (await _dataContext.Staff.Where(c => c.StaffId == user.UserTypeId).FirstOrDefaultAsync()).FullName;
                }

                var userName = currentUserName.Split();

                var emailMessage1 = "Please use the code below to complete your registration and activate your account.";

                var otpCode = totp.ComputeTotp();
                var emailMessage2 = otpCode;

                string emailBody = _globalRepository.GetMailBodyTemplate(userName[0], userName[1], "", emailMessage1, emailMessage2, "activationMobile.html");

                //SEND MAIL TO CUSTOMER TO VERIFY EMAIL
                MailModel mailObj = new MailModel(_configuration.GetValue<string>("AyudaEmailAddress"), _configuration.GetValue<string>("AyudaEmailName"), user.Email, emailSubject, emailBody);
                var response = await _mailController.SendMail(mailObj);
                if (response.StatusCode.Equals(HttpStatusCode.Accepted))
                {
                    var cacheEntry = otpCode;
                    var cacheExpiryOptions = new MemoryCacheEntryOptions
                    {
                        AbsoluteExpiration = DateTime.Now.AddMinutes(30),
                        Priority = CacheItemPriority.High,
                        SlidingExpiration = TimeSpan.FromMinutes(2),
                        Size = 1024,
                    };

                    string code = string.Empty;
                    _cache.TryGetValue(user.Email, out code);

                    if (code is null)
                    {
                        _cache.Set(user.Email, otpCode, cacheExpiryOptions);
                    }
                    else
                    {
                        return new ReturnResponse()
                        {
                            StatusCode = Utils.BadRequest,
                            StatusMessage = "OTP has already been sent!!!",
                        };
                    }

                    _cache.Set(user.Email, otpCode, cacheExpiryOptions);
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.Success,
                        StatusMessage = "Email Verification Code Sent Successfully!!!",
                        ObjectValue = user
                    };
                }
                else
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.MailFailure
                    };
                }
            }

            return new ReturnResponse()
            {
                StatusCode = Utils.NotFound
            };
        }

        public static class CacheKeys
        {
            public static string Entry { get { return "_Entry"; } }
            public static string CallbackEntry { get { return "_Callback"; } }
            public static string CallbackMessage { get { return "_CallbackMessage"; } }
            public static string Parent { get { return "_Parent"; } }
            public static string Child { get { return "_Child"; } }
            public static string DependentMessage { get { return "_DependentMessage"; } }
            public static string DependentCTS { get { return "_DependentCTS"; } }
            public static string Ticks { get { return "_Ticks"; } }
            public static string CancelMsg { get { return "_CancelMsg"; } }
            public static string CancelTokenSource { get { return "_CancelTokenSource"; } }
        }
    }
}
